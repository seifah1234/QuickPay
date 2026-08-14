using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Database;
using QuickPay.DAL.Entity;
using QuickPay.DAL.Repo.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repo.Implementation
{
    public class NotificationRepo:INotificationRepo
    {
        private readonly ProjectDBContext _context;

        public NotificationRepo(ProjectDBContext context)
        {
            _context = context;
        }

        

       

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
