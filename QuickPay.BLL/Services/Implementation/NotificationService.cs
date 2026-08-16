using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IRealtimeNotifier realtimeNotifier)
        {
            _unitOfWork = unitOfWork;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task NotifyAsync(
            int userId,
            string type,
            string message,
            CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message
            };

            await _unitOfWork.Notifications.AddAsync(
                notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _realtimeNotifier.PushToUserAsync(
                userId, type, message, cancellationToken);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var notifications = await _unitOfWork.Notifications
                .GetByUserIdAsync(userId, cancellationToken);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task<bool> MarkAsReadAsync(
            int notificationId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var notification = await _unitOfWork.Notifications
                .GetByIdAsync(notificationId, cancellationToken);

            if (notification is null || notification.UserId != userId)
            {
                return false;
            }

            notification.IsRead = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
