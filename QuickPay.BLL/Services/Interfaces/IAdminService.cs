using QuickPay.BLL.DTOs.Admin;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AdminUserDto>> GetUsersAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Flips a user's IsActive flag. Refuses if the target is the
        /// calling admin themselves, or the last remaining active Admin -
        /// same "don't let admins lock everyone out, including
        /// themselves" logic as SharedWallet's last-admin protection.
        /// </summary>
        Task ToggleUserActiveAsync(
            int targetUserId,
            int currentAdminId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AdminWalletDto>> GetWalletsAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AdminTransactionDto>> GetTransactionsAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
