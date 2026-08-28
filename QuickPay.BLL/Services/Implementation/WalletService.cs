using Microsoft.EntityFrameworkCore;
using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public WalletService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<WalletDto>> GetMyWalletsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var wallets = await _unitOfWork.Wallets
                .GetByUserIdAsync(userId, cancellationToken);

            return wallets.Select(ToDto);
        }

        public async Task<WalletDto> CreateWalletAsync(
            CreateWalletDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Wallet name is required.");
            }

            var wallet = new Wallet
            {
                UserId = request.CurrentUserId,
                Name = request.Name.Trim(),
                Balance = 0,
                Currency = "EGP",
                IsActive = true
            };

            await _unitOfWork.Wallets.AddAsync(wallet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notificationService.NotifyAsync(
    wallet.UserId,
    "WalletCreated",
    $"A new wallet named \"{wallet.Name}\" has been created.",
    cancellationToken);

            return ToDto(wallet);
        }

        public async Task<WalletDto> RenameWalletAsync(
            RenameWalletDto request,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOwnedWalletOrThrow(
                request.WalletId,
                request.CurrentUserId,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(request.NewName))
            {
                throw new ArgumentException(
                    "Wallet name is required.");
            }

            wallet.Name = request.NewName.Trim();
            wallet.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notificationService.NotifyAsync(
    wallet.UserId,
    "WalletRenamed",
    $"Your wallet was renamed to \"{wallet.Name}\".",
    cancellationToken);

            return ToDto(wallet);
        }

        public async Task DeleteWalletAsync(
            int walletId,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var wallet = await GetOwnedWalletOrThrow(
                walletId,
                currentUserId,
                cancellationToken);

            if (wallet.Balance > 0)
            {
                throw new InvalidOperationException(
                    "Cannot delete a wallet with a remaining balance. Withdraw or transfer the funds first.");
            }

            var remainingWallets = await _unitOfWork.Wallets
                .GetByUserIdAsync(currentUserId, cancellationToken);

            if (remainingWallets.Count() <= 1)
            {
                throw new InvalidOperationException(
                    "You must keep at least one wallet.");
            }

            // Soft delete: other modules (Transfer, Transaction History)
            // still reference this wallet by Id through past transactions.
            wallet.IsActive = false;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notificationService.NotifyAsync(
    wallet.UserId,
    "WalletDeleted",
    $"Your wallet \"{wallet.Name}\" has been closed.",
    cancellationToken);
        }

        public async Task<WalletDto> DepositAsync(
            DepositWithdrawRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return await ApplyBalanceChangeAsync(
                request,
                TransactionType.Deposit,
                isDeposit: true,
                cancellationToken);
        }

        public async Task<WalletDto> WithdrawAsync(
            DepositWithdrawRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return await ApplyBalanceChangeAsync(
                request,
                TransactionType.Withdraw,
                isDeposit: false,
                cancellationToken);
        }

        private async Task<WalletDto> ApplyBalanceChangeAsync(
            DepositWithdrawRequestDto request,
            TransactionType type,
            bool isDeposit,
            CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException(
                    "Amount must be greater than zero.");
            }

            var wallet = await GetOwnedWalletOrThrow(
                request.WalletId,
                request.CurrentUserId,
                cancellationToken);

            if (!isDeposit && wallet.Balance < request.Amount)
            {
                throw new InvalidOperationException(
                    "Insufficient balance.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                wallet.Balance += isDeposit ? request.Amount : -request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                // Simulated deposit/withdraw: only one real account is
                // involved, so it self-references on both sides of the
                // Transaction row (kept for the shared history feed).
                var transaction = new Transaction
                {
                    FromAccountId = wallet.Id,
                    ToAccountId = wallet.Id,
                    Amount = request.Amount,
                    Type = type,
                    Status = TransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Transactions.AddAsync(
                    transaction, cancellationToken);

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException(
                        "This wallet was updated at the same time by another operation. Please try again.");
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                string action = isDeposit ? "deposited" : "withdrew";
                string notifType = isDeposit ? "DepositSucceeded" : "WithdrawSucceeded";

                await _notificationService.NotifyAsync(
                    request.CurrentUserId,
                    notifType,
                    $"You {action} {request.Amount} EGP {(isDeposit ? "into" : "from")} \"{wallet.Name}\".",
                    cancellationToken);

                return ToDto(wallet);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private async Task<Wallet> GetOwnedWalletOrThrow(
            int walletId,
            int currentUserId,
            CancellationToken cancellationToken)
        {
            var wallet = await _unitOfWork.Wallets
                .GetByIdAsync(walletId, cancellationToken);

            if (wallet is null)
            {
                throw new KeyNotFoundException("Wallet was not found.");
            }

            if (wallet.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to access this wallet.");
            }

            return wallet;
        }

        private static WalletDto ToDto(Wallet wallet)
        {
            return new WalletDto
            {
                Id = wallet.Id,
                Name = wallet.Name,
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                IsActive = wallet.IsActive
            };
        }
    }
}
