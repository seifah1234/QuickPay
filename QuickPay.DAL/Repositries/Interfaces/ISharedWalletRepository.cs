using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface ISharedWalletRepository
    {
        Task<SharedWallet?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<SharedWallet?> GetWithMembersAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<SharedWallet>> GetByMemberUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            SharedWallet sharedWallet,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<SharedWallet>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default);
    }
}
