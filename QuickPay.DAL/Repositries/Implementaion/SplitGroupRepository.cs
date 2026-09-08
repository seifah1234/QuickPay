using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class SplitGroupRepository : ISplitGroupRepository
    {
        private readonly QuickPayDbContext _context;

        public SplitGroupRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<SplitGroup?> GetWithDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SplitGroup>()
                .Include(x => x.Payment)
                    .ThenInclude(p => p.User)
                .Include(x => x.Payment)
                    .ThenInclude(p => p.SettlementWallet)
                .Include(x => x.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<SplitGroup>> GetForUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SplitGroup>()
                .Include(x => x.Payment)
                    .ThenInclude(p => p.User)
                .Include(x => x.Participants)
                    .ThenInclude(p => p.User)
                .Where(x =>
                    x.Payment.UserId == userId ||
                    x.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            SplitGroup splitGroup,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<SplitGroup>()
                .AddAsync(splitGroup, cancellationToken);
        }
    }
}
