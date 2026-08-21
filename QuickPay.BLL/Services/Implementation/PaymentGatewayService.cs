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
        private readonly ILinkedAccountsService _linkedAccountsService;

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

            var merchantReference = $"DEP-{request.WalletId}-{DateTime.UtcNow.Ticks}";

            var chargeResult = await _provider.InitiateChargeAsync(
                new GatewayChargeRequest
                {
                    UserId = request.CurrentUserId,
                    Amount = request.Amount,
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
            if (!_provider.VerifyWebhookSignature(rawBody, query, receivedSignature))
            {
                return false;
            }

            var webhookEvent = _provider.ParseWebhookEvent(rawBody, query);

            if (webhookEvent.EventType == GatewayWebhookEventType.CardToken)
            {
                return await HandleCardTokenCallbackAsync(webhookEvent, cancellationToken);
            }

            return await HandleTransactionCallbackAsync(webhookEvent, cancellationToken);
        }

        // TOKEN callback: the second, separate webhook Paymob fires only
        // when "Save Card" is enabled and a card was actually saved. This
        // is the only place CardToken/MaskedPan/CardSubType exist.
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
                // Either the matching TRANSACTION callback hasn't been
                // processed yet (race condition - Paymob should retry a
                // non-200 response) or this token isn't for a link-card
                // flow we know about.
                return false;
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
                if (!webhookEvent.IsSuccessful)
                {
                    pgt.Status = PaymentGatewayTransactionStatus.Failed;
                    pgt.CompletedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _notificationService.NotifyAsync(
                        pgt.UserId,
                        type: "CardLinkFailed",
                        message: "We couldn't verify your card. Nothing was charged.",
                        cancellationToken);

                    return true;
                }

                // Remember Paymob's ids so a later, separate TOKEN callback
                // can be matched back to this row. Stays Pending until then
                // - unless the provider already attached card data to this
                // same event (the Fake gateway does this, real Paymob does
                // not), in which case finish immediately.
                pgt.ProviderOrderId = webhookEvent.ProviderOrderId;
                pgt.ProviderTransactionId = webhookEvent.ProviderTransactionId;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(webhookEvent.CardToken))
                {
                    return await CompleteLinkCardAsync(pgt, webhookEvent, cancellationToken);
                }

                return true;
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

                if (webhookEvent.IsSuccessful)
                {
                    var wallet = await _unitOfWork.Wallets.GetByIdAsync(
                        pgt.WalletId!.Value, cancellationToken);

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

        private async Task<bool> CompleteLinkCardAsync(
            PaymentGatewayTransaction pgt,
            GatewayWebhookEvent webhookEvent,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(webhookEvent.CardToken))
            {
                return false;
            }

            BankAccount linkedAccount;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                pgt.Status = PaymentGatewayTransactionStatus.Succeeded;
                pgt.CompletedAt = DateTime.UtcNow;

                var linkedAccountDto = await _linkedAccountsService.LinkAsync(
                    new LinkBankAccountRequestDto
                    {
                        CurrentUserId = pgt.UserId,
                        DisplayName = BuildCardDisplayName(
                            webhookEvent.CardSubType, webhookEvent.MaskedPan),
                        MaskedNumber = webhookEvent.MaskedPan ?? "****",
                        GatewayToken = webhookEvent.CardToken
                    },
                    cancellationToken);

                linkedAccount = new BankAccount
                {
                    Id = linkedAccountDto.Id,
                    DisplayName = linkedAccountDto.DisplayName,
                    MaskedNumber = linkedAccountDto.MaskedNumber
                };

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            await NotifyLinkCardOutcomeAsync(pgt, linkedAccount, cancellationToken);

            return true;
        }

        private async Task NotifyLinkCardOutcomeAsync(
            PaymentGatewayTransaction pgt,
            BankAccount linkedAccount,
            CancellationToken cancellationToken)
        {
            // Best-effort refund of the nominal verification charge - a
            // network call, so it happens after the DB transaction commits
            // and never blocks the card from being linked.
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
