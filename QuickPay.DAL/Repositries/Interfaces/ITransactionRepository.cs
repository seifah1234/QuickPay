using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Transaction>> GetByAccountIdAsync(
            int accountId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Transaction>> GetHistoryForUserAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
