using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Payment payment,
            CancellationToken cancellationToken = default);
    }
}
