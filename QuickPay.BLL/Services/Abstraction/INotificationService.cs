using QuickPay.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Abstraction
{
    public interface INotificationService
    {
        Task NotifyAsync(int userId, string type, string message);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
    }
}
