using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly QuickPayDbContext _context;

        public NotificationRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            await _context.Notifications.AddAsync(
                notification, cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Notification?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }
    }
}
