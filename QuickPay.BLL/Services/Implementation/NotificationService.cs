using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Abstraction;
using QuickPay.DAL.Entity;
using QuickPay.DAL.Repo.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Implementation
{
    public class NotificationService:INotificationService
    {
        private readonly INotificationRepo _notificationRepo;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public NotificationService(
            INotificationRepo notificationRepo,
            IRealtimeNotifier realtimeNotifier)
        {
            _notificationRepo = notificationRepo;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task NotifyAsync(int userId, string type, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message
            };

            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveChangesAsync();

            await _realtimeNotifier.PushToUserAsync(userId, type, message);
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepo.GetByUserIdAsync(userId);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            await _notificationRepo.MarkAsReadAsync(notificationId);
            await _notificationRepo.SaveChangesAsync();
        }
    }
}

