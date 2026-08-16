using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(
            Notification notification,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Notification>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<Notification?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
