using QuickPay.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repo.Abstraction
{
    public interface INotificationRepo
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int id);
        Task MarkAsReadAsync(int id);
        Task SaveChangesAsync();
    }
}
