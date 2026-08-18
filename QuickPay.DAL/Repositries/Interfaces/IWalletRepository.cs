using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Wallet>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Wallet wallet,
            CancellationToken cancellationToken = default);

        void Remove(Wallet wallet);
    }
}
