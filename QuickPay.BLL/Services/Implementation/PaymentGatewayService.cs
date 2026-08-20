using Microsoft.EntityFrameworkCore;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGatewayProvider _provider;
        private readonly INotificationService _notificationService;

        public PaymentGatewayService(
            IUnitOfWork unitOfWork,
            IPaymentGatewayProvider provider,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _provider = provider;
            _notificationService = notificationService;
        }

        public async Task<GatewayInitiationResultDto> InitiateDepositAsync(
            InitiateDepositRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request.Amount <= 0)
            {
                return Fail("Amount must be greater than zero.");
            }

            var isAuthorized = await _unitOfWork.FinancialAccounts
                .IsUserAuthorizedForAccountAsync(
                    request.WalletId, request.CurrentUserId, cancellationToken);

            if (!isAuthorized)
            {
                return Fail("You are not authorized to deposit into this wallet.");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(
                request.CurrentUserId, cancellationToken);

            if (user is null)
            {
                return Fail("Account not found.");
            }

            var chargeResult = await _provider.InitiateChargeAsync(
                new DTOs.PaymentGateway.GatewayChargeRequest
                {
                    UserId = request.CurrentUserId,
                    Amount = request.Amount,
                    MerchantReference = $"DEP-{request.WalletId}-{DateTime.UtcNow.Ticks}",
                    PayerFullName = user.UserName,
                    PayerEmail = user.Email,
                    PayerPhoneNumber = user.PhoneNumber
                },
                cancellationToken);

            if (!chargeResult.IsSuccess)
            {
                return Fail(
                    chargeResult.ErrorMessage ??
                    "Could not start the deposit. Please try again.");
            }

            var pgt = new PaymentGatewayTransaction
            {
                UserId = request.CurrentUserId,
                WalletId = request.WalletId,
                Direction = PaymentGatewayDirection.Deposit,
                GatewayTransactionId = chargeResult.GatewayTransactionId,
                Amount = request.Amount,
                Status = PaymentGatewayTransactionStatus.Pending
            };

            await _unitOfWork.PaymentGatewayTransactions.AddAsync(
                pgt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GatewayInitiationResultDto
            {
                IsSuccess = true,
                Message = "Redirecting to complete your deposit.",
                CheckoutUrl = chargeResult.CheckoutUrl
            };
        }

        public async Task<GatewayInitiationResultDto> InitiateWithdrawAsync(
            InitiateWithdrawRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request.Amount <= 0)
            {
                return Fail("Amount must be greater than zero.");
            }

            var isAuthorized = await _unitOfWork.FinancialAccounts
                .IsUserAuthorizedForAccountAsync(
                    request.WalletId, request.CurrentUserId, cancellationToken);

            if (!isAuthorized)
            {
                return Fail("You are not authorized to withdraw from this wallet.");
            }

            var wallet = await _unitOfWork.Wallets.GetByIdAsync(
                request.WalletId, cancellationToken);

            if (wallet is null)
            {
                return Fail("Wallet was not found.");
            }

            var bankAccount = await _unitOfWork.BankAccounts.GetByIdAsync(
                request.BankAccountId, cancellationToken);

            if (bankAccount is null ||
                bankAccount.UserId != request.CurrentUserId ||
                !bankAccount.IsActive)
            {
                return Fail("Linked account was not found.");
            }

            var alreadyPending = await _unitOfWork.PaymentGatewayTransactions
                .GetPendingWithdrawAmountAsync(request.WalletId, cancellationToken);

            if (wallet.Balance - alreadyPending < request.Amount)
            {
                return Fail("Insufficient available balance.");
            }

            var payoutResult = await _provider.InitiatePayoutAsync(
                new DTOs.PaymentGateway.GatewayPayoutRequest
                {
                    UserId = request.CurrentUserId,
                    Amount = request.Amount,
                    MerchantReference = $"WD-{request.WalletId}-{DateTime.UtcNow.Ticks}",
                    DestinationToken = bankAccount.GatewayToken
                },
                cancellationToken);

            if (!payoutResult.IsSuccess)
            {
                return Fail(
                    payoutResult.ErrorMessage ??
                    "Could not start the withdrawal. Please try again.");
            }

            var pgt = new PaymentGatewayTransaction
            {
                UserId = request.CurrentUserId,
                WalletId = request.WalletId,
                BankAccountId = request.BankAccountId,
                Direction = PaymentGatewayDirection.Withdraw,
                GatewayTransactionId = payoutResult.GatewayTransactionId,
                Amount = request.Amount,
                Status = PaymentGatewayTransactionStatus.Pending
            };

            await _unitOfWork.PaymentGatewayTransactions.AddAsync(
                pgt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GatewayInitiationResultDto
            {
                IsSuccess = true,
                Message = "Withdrawal started. You'll be notified once it's complete."
            };
        }

        public async Task<bool> HandleWebhookAsync(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature,
            CancellationToken cancellationToken = default)
        {
            if (!_provider.VerifyWebhookSignature(rawBody, query, receivedSignature))
            {
                return false;
            }

            var webhookEvent = _provider.ParseWebhookEvent(rawBody, query);

            var pgt = await _unitOfWork.PaymentGatewayTransactions
                .GetByGatewayTransactionIdAsync(
                    webhookEvent.GatewayTransactionId, cancellationToken);

            if (pgt is null)
            {
                return false;
            }

            if (pgt.Status != PaymentGatewayTransactionStatus.Pending)
            {
                return true;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                pgt.Status = webhookEvent.IsSuccessful
                    ? PaymentGatewayTransactionStatus.Succeeded
                    : PaymentGatewayTransactionStatus.Failed;
                pgt.CompletedAt = DateTime.UtcNow;

                if (webhookEvent.IsSuccessful)
                {
                    var wallet = await _unitOfWork.Wallets.GetByIdAsync(
                        pgt.WalletId, cancellationToken);

                    if (wallet is null)
                    {
                        throw new InvalidOperationException(
                            $"Wallet {pgt.WalletId} referenced by gateway transaction {pgt.Id} no longer exists.");
                    }

                    wallet.Balance += pgt.Direction == PaymentGatewayDirection.Deposit
                        ? pgt.Amount
                        : -pgt.Amount;
                    wallet.UpdatedAt = DateTime.UtcNow;

                    var ledgerTransaction = new Transaction
                    {
                        FromAccountId = wallet.Id,
                        ToAccountId = wallet.Id,
                        Amount = pgt.Amount,
                        Type = pgt.Direction == PaymentGatewayDirection.Deposit
                            ? TransactionType.Deposit
                            : TransactionType.Withdraw,
                        Status = TransactionStatus.Completed,
                        PaymentGatewayTransactionId = pgt.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Transactions.AddAsync(
                        ledgerTransaction, cancellationToken);
                }

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException(
                        "The wallet was updated at the same time by another operation.");
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            await _notificationService.NotifyAsync(
                pgt.UserId,
                type: pgt.Direction == PaymentGatewayDirection.Deposit
                    ? "DepositCompleted"
                    : "WithdrawCompleted",
                message: webhookEvent.IsSuccessful
                    ? $"Your {pgt.Direction.ToString().ToLower()} of {pgt.Amount:N2} EGP was successful."
                    : $"Your {pgt.Direction.ToString().ToLower()} of {pgt.Amount:N2} EGP failed.",
                cancellationToken);

            return true;
        }

        private static GatewayInitiationResultDto Fail(string message) => new()
        {
            IsSuccess = false,
            Message = message
        };
    }
}
