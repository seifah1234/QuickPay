using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class SharedWalletRepository : ISharedWalletRepository
    {
        private readonly QuickPayDbContext _context;

        public SharedWalletRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<SharedWallet?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SharedWallet>()
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<SharedWallet?> GetWithMembersAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SharedWallet>()
                .Include(x => x.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<IEnumerable<SharedWallet>> GetByMemberUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SharedWallet>()
                .Include(x => x.Members)
                    .ThenInclude(m => m.User)
                .Where(x =>
                    x.IsActive &&
                    x.Members.Any(m => m.UserId == userId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            SharedWallet sharedWallet,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<SharedWallet>()
                .AddAsync(sharedWallet, cancellationToken);
        }

        public async Task<IEnumerable<SharedWallet>> GetAllAsync(int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
        {
            return await _context.Set<SharedWallet>()
                .Include(w => w.Members)
                .ThenInclude(m => m.User)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
