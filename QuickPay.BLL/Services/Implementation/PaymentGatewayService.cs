using Microsoft.EntityFrameworkCore;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;
using System.Collections.Concurrent;

namespace QuickPay.BLL.Services.Implementation
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGatewayProvider _provider;
        private readonly INotificationService _notificationService;
        private readonly ILinkedAccountsService _linkedAccountsService;

        private static readonly ConcurrentDictionary<string, GatewayWebhookEvent> _pendingTokens = new();
        private const decimal LinkCardVerificationAmount = 1.00m;

        public PaymentGatewayService(
            IUnitOfWork unitOfWork,
            IPaymentGatewayProvider provider,
            INotificationService notificationService,
            ILinkedAccountsService linkedAccountsService)
        {
            _unitOfWork = unitOfWork;
            _provider = provider;
            _notificationService = notificationService;
            _linkedAccountsService = linkedAccountsService;
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

            // Deposit-with-saved-card: verify the BankAccount is really
            // this user's own before letting its token be charged - the
            // same ownership check every other feature in this app makes
            // before touching an account it didn't explicitly search for.
            string? savedCardToken = null;

            if (request.BankAccountId.HasValue)
            {
                var bankAccount = await _unitOfWork.BankAccounts.GetByIdAsync(
                    request.BankAccountId.Value, cancellationToken);

                if (bankAccount is null ||
                    bankAccount.UserId != request.CurrentUserId ||
                    !bankAccount.IsActive)
                {
                    return Fail("Linked account was not found.");
                }

                savedCardToken = bankAccount.GatewayToken;
            }

            var merchantReference = $"DEP-{request.WalletId}-{DateTime.UtcNow.Ticks}";

            var chargeResult = await _provider.InitiateChargeAsync(
                new GatewayChargeRequest
                {
                    UserId = request.CurrentUserId,
                    Amount = request.Amount,
                    MerchantReference = merchantReference,
                    PayerFullName = user.UserName,
                    PayerEmail = user.Email,
                    PayerPhoneNumber = user.PhoneNumber,
                    SavedCardToken = savedCardToken
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
                GatewayTransactionId = merchantReference,
                Amount = request.Amount,
                Status = PaymentGatewayTransactionStatus.Pending
            };

            await _unitOfWork.PaymentGatewayTransactions.AddAsync(
                pgt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GatewayInitiationResultDto
            {
                IsSuccess = true,
                Message = chargeResult.CheckoutUrl is not null
                    ? "Redirecting to complete your deposit."
                    : "Deposit started with your saved card. You'll be notified once it's confirmed.",
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

            var merchantReference = $"WD-{request.WalletId}-{DateTime.UtcNow.Ticks}";

            var payoutResult = await _provider.InitiatePayoutAsync(
                new GatewayPayoutRequest
                {
                    UserId = request.CurrentUserId,
                    Amount = request.Amount,
                    MerchantReference = merchantReference,
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
                GatewayTransactionId = merchantReference,
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

        public async Task<GatewayInitiationResultDto> InitiateLinkCardAsync(
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(
                currentUserId, cancellationToken);

            if (user is null)
            {
                return Fail("Account not found.");
            }

            var merchantReference = $"LINK-{currentUserId}-{DateTime.UtcNow.Ticks}";

            var chargeResult = await _provider.InitiateChargeAsync(
                new GatewayChargeRequest
                {
                    UserId = currentUserId,
                    Amount = LinkCardVerificationAmount,
                    MerchantReference = merchantReference,
                    PayerFullName = user.UserName,
                    PayerEmail = user.Email,
                    PayerPhoneNumber = user.PhoneNumber
                },
                cancellationToken);

            if (!chargeResult.IsSuccess)
            {
                return Fail(
                    chargeResult.ErrorMessage ??
                    "Could not start card verification. Please try again.");
            }

            var pgt = new PaymentGatewayTransaction
            {
                UserId = currentUserId,
                WalletId = null,
                Direction = PaymentGatewayDirection.LinkCard,
                GatewayTransactionId = merchantReference,
                Amount = LinkCardVerificationAmount,
                Status = PaymentGatewayTransactionStatus.Pending
            };

            await _unitOfWork.PaymentGatewayTransactions.AddAsync(
                pgt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new GatewayInitiationResultDto
            {
                IsSuccess = true,
                Message =
                    $"Redirecting to verify your card with a {LinkCardVerificationAmount:N2} EGP charge (refunded automatically).",
                CheckoutUrl = chargeResult.CheckoutUrl
            };
        }

        public async Task<bool> HandleWebhookAsync(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("PAYMOB WEBHOOK");
            Console.WriteLine($"Raw Body: {rawBody}");
            Console.WriteLine($"Query: {string.Join(", ", query.Select(x => $"{x.Key}={x.Value}"))}");
            Console.WriteLine($"Signature: {receivedSignature}");
            Console.WriteLine("=================================");

            if (!_provider.VerifyWebhookSignature(rawBody, query, receivedSignature))
            {
                Console.WriteLine("Signature verification FAILED");
                return false;
            }

            Console.WriteLine("Signature verified OK");
            var webhookEvent = _provider.ParseWebhookEvent(rawBody, query);

            if (webhookEvent.EventType == GatewayWebhookEventType.CardToken)
            {
                return await HandleCardTokenCallbackAsync(webhookEvent, cancellationToken);
            }

            return await HandleTransactionCallbackAsync(webhookEvent, cancellationToken);
        }

        private async Task<bool> HandleCardTokenCallbackAsync(
    GatewayWebhookEvent webhookEvent,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(webhookEvent.ProviderOrderId))
            {
                return false;
            }

            var pgt = await _unitOfWork.PaymentGatewayTransactions
                .GetByProviderOrderIdAsync(
                    webhookEvent.ProviderOrderId, cancellationToken);

            if (pgt is null ||
                pgt.Direction != PaymentGatewayDirection.LinkCard ||
                pgt.Status != PaymentGatewayTransactionStatus.Pending)
            {

                _pendingTokens[webhookEvent.ProviderOrderId] = webhookEvent;
                return true;
            }

            return await CompleteLinkCardAsync(pgt, webhookEvent, cancellationToken);
        }

        private async Task<bool> HandleTransactionCallbackAsync(
    GatewayWebhookEvent webhookEvent,
    CancellationToken cancellationToken)
        {
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

            if (pgt.Direction == PaymentGatewayDirection.LinkCard)
            {
                return await HandleLinkCardTransactionAsync(pgt, webhookEvent, cancellationToken);
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                pgt.Status = webhookEvent.IsSuccessful
                    ? PaymentGatewayTransactionStatus.Succeeded
                    : PaymentGatewayTransactionStatus.Failed;
                pgt.CompletedAt = DateTime.UtcNow;
                pgt.ProviderTransactionId = webhookEvent.ProviderTransactionId;
                pgt.ProviderOrderId = webhookEvent.ProviderOrderId;

                if (webhookEvent.IsSuccessful && pgt.WalletId.HasValue)
                {
                    var wallet = await _unitOfWork.Wallets.GetByIdAsync(
                        pgt.WalletId.Value, cancellationToken);

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

                await _unitOfWork.SaveChangesAsync(cancellationToken);
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

        private async Task<bool> HandleLinkCardTransactionAsync(
    PaymentGatewayTransaction pgt,
    GatewayWebhookEvent webhookEvent,
    CancellationToken cancellationToken)
        {
            if (!webhookEvent.IsSuccessful)
            {
                pgt.Status = PaymentGatewayTransactionStatus.Failed;
                pgt.CompletedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            if (_pendingTokens.TryRemove(webhookEvent.ProviderOrderId!, out var tokenEvent))
            {
                return await CompleteLinkCardAsync(pgt, tokenEvent, cancellationToken);
            }

            pgt.ProviderOrderId = webhookEvent.ProviderOrderId;
            pgt.ProviderTransactionId = webhookEvent.ProviderTransactionId;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Console.WriteLine(
                    $"Transaction succeeded. Waiting for TOKEN callback. " +
                    $"OrderId: {webhookEvent.ProviderOrderId}");
            return true;
        }

        private async Task<bool> CreateBankAccountFromTransactionAsync(
            PaymentGatewayTransaction pgt,
            GatewayWebhookEvent webhookEvent,
            string maskedPan,
            string cardSubType,
            CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                pgt.Status = PaymentGatewayTransactionStatus.Succeeded;
                pgt.CompletedAt = DateTime.UtcNow;
                pgt.ProviderOrderId = webhookEvent.ProviderOrderId;
                pgt.ProviderTransactionId = webhookEvent.ProviderTransactionId;

                var bankAccount = new BankAccount
                {
                    UserId = pgt.UserId,
                    DisplayName = BuildCardDisplayName(cardSubType, maskedPan),
                    MaskedNumber = maskedPan,
                    GatewayToken = webhookEvent.ProviderTransactionId ?? pgt.GatewayTransactionId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.BankAccounts.AddAsync(bankAccount, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                Console.WriteLine($"BankAccount created: {maskedPan}, Token: {bankAccount.GatewayToken}");

                await NotifyLinkCardOutcomeAsync(pgt, bankAccount, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                Console.WriteLine($"Error creating BankAccount: {ex.Message}");
                throw;
            }
        }


        private async Task<bool> CompleteLinkCardAsync(
    PaymentGatewayTransaction pgt,
    GatewayWebhookEvent webhookEvent,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(webhookEvent.CardToken))
            {
                return false;
            }

            try
            {
                pgt.Status = PaymentGatewayTransactionStatus.Succeeded;
                pgt.CompletedAt = DateTime.UtcNow;
                pgt.ProviderTransactionId = webhookEvent.ProviderTransactionId;
                pgt.ProviderOrderId = webhookEvent.ProviderOrderId;

                var bankAccount = new BankAccount
                {
                    UserId = pgt.UserId,
                    DisplayName = BuildCardDisplayName(
                        webhookEvent.CardSubType, webhookEvent.MaskedPan),
                    MaskedNumber = webhookEvent.MaskedPan ?? "****",
                    GatewayToken = webhookEvent.CardToken,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.BankAccounts.AddAsync(bankAccount, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await NotifyLinkCardOutcomeAsync(pgt, bankAccount, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CompleteLinkCardAsync: {ex.Message}");
                return false;
            }
        }

        private async Task NotifyLinkCardOutcomeAsync(
            PaymentGatewayTransaction pgt,
            BankAccount linkedAccount,
            CancellationToken cancellationToken)
        {
            var refundResult = await _provider.RefundAsync(
                new GatewayRefundRequest
                {
                    GatewayTransactionId =
                        pgt.ProviderTransactionId ?? pgt.GatewayTransactionId,
                    Amount = pgt.Amount
                },
                cancellationToken);

            await _notificationService.NotifyAsync(
                pgt.UserId,
                type: "CardLinked",
                message: refundResult.IsSuccess
                    ? $"{linkedAccount.MaskedNumber} was linked. The {pgt.Amount:N2} EGP verification charge was refunded."
                    : $"{linkedAccount.MaskedNumber} was linked, but the {pgt.Amount:N2} EGP verification refund failed and needs manual follow-up.",
                cancellationToken);
        }
        private static string BuildCardDisplayName(string? cardSubType, string? maskedPan)
        {
            var brand = string.IsNullOrWhiteSpace(cardSubType) ? "Card" : cardSubType;

            return string.IsNullOrWhiteSpace(maskedPan)
                ? brand
                : $"{brand} •••• {maskedPan}";
        }

        private static GatewayInitiationResultDto Fail(string message) => new()
        {
            IsSuccess = false,
            Message = message
        };
    }
}
