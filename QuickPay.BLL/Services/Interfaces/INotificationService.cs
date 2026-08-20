using QuickPay.BLL.DTOs;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotifyAsync(
            int userId,
            string type,
            string message,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<bool> MarkAsReadAsync(
            int notificationId,
            int userId,
            CancellationToken cancellationToken = default);
    }
}
