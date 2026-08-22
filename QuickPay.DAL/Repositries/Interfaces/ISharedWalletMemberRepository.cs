using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface ISharedWalletMemberRepository
    {
        Task<SharedWalletMember?> GetAsync(
            int sharedWalletId,
            int userId,
            CancellationToken cancellationToken = default);

        Task<int> CountAdminsAsync(
            int sharedWalletId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            SharedWalletMember member,
            CancellationToken cancellationToken = default);

        void Remove(SharedWalletMember member);
    }
}
