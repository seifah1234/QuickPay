using QuickPay.BLL.DTOs.Admin;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync(
            CancellationToken cancellationToken = default)
        {
            var users = (await _unitOfWork.Users.GetAllAsync(cancellationToken)).ToList();
            var wallets = (await _unitOfWork.FinancialAccounts.GetAllAsync(cancellationToken)).ToList();

            // Cheap total count for the dashboard - page 1 at a large
            // page size rather than adding a separate CountAsync method
            // just for one number on an overview screen.
            var transactionSample = await _unitOfWork.Transactions
                .GetAllAsync(1, 100000, cancellationToken);

            return new AdminDashboardDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                TotalWallets = wallets.Count,
                TotalBalanceAcrossWallets = wallets.Sum(w => w.Balance),
                TotalTransactions = transactionSample.Count()
            };
        }

        public async Task<IEnumerable<AdminUserDto>> GetUsersAsync(
            CancellationToken cancellationToken = default)
        {
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);

            return users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                IsPhoneVerified = u.IsPhoneVerified,
                CreatedAt = u.CreatedAt
            });
        }

        public async Task ToggleUserActiveAsync(
            int targetUserId,
            int currentAdminId,
            CancellationToken cancellationToken = default)
        {
            if (targetUserId == currentAdminId)
            {
                throw new InvalidOperationException(
                    "You can't disable your own account.");
            }

            var target = await _unitOfWork.Users.GetByIdAsync(
                targetUserId, cancellationToken);

            if (target is null)
            {
                throw new KeyNotFoundException("User was not found.");
            }

            if (target.IsAdmin && target.IsActive)
            {
                var allUsers = await _unitOfWork.Users.GetAllAsync(cancellationToken);
                var activeAdminCount = allUsers.Count(
                    u => u.IsAdmin && u.IsActive);

                if (activeAdminCount <= 1)
                {
                    throw new InvalidOperationException(
                        "Can't disable the last active Admin - promote another user to Admin first.");
                }
            }

            target.IsActive = !target.IsActive;
            target.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<AdminWalletDto>> GetWalletsAsync(
            CancellationToken cancellationToken = default)
        {
            var accounts = await _unitOfWork.FinancialAccounts.GetAllAsync(cancellationToken);

            return accounts.Select(a => new AdminWalletDto
            {
                Id = a.Id,
                Name = a.Name,
                Type = a is Wallet ? "Personal" : "Shared",
                Balance = a.Balance,
                Currency = a.Currency,
                IsActive = a.IsActive,
                OwnerDisplay = GetOwnerDisplay(a),
                CreatedAt = a.CreatedAt
            });
        }

        public async Task<IEnumerable<AdminTransactionDto>> GetTransactionsAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var transactions = await _unitOfWork.Transactions.GetAllAsync(
                pageNumber, pageSize, cancellationToken);

            return transactions.Select(t => new AdminTransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                Status = t.Status.ToString(),
                FromAccountName = t.FromAccount?.Name ?? $"Account #{t.FromAccountId}",
                ToAccountName = t.ToAccount?.Name ?? $"Account #{t.ToAccountId}",
                CreatedAt = t.CreatedAt
            });
        }

        private static string GetOwnerDisplay(FinancialAccount account)
        {
            if (account is Wallet wallet)
            {
                return wallet.User?.UserName ?? $"User #{wallet.UserId}";
            }

            if (account is SharedWallet sharedWallet)
            {
                var members = sharedWallet.Members
                    ?.Select(m => m.User?.UserName ?? $"User #{m.UserId}")
                    ?? Enumerable.Empty<string>();

                return string.Join(", ", members);
            }

            return "-";
        }
    }
}
