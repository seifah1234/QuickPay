using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IBankAccountRepository
    {
        Task AddAsync(
            BankAccount bankAccount,
            CancellationToken cancellationToken = default);

        Task<BankAccount?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<BankAccount>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
