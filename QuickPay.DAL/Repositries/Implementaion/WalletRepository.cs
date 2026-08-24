
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class WalletRepository : IWalletRepository
    {
        private readonly QuickPayDbContext _context;

        public WalletRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Wallet>()
                .FirstOrDefaultAsync(
                    w => w.Id == id,
                    cancellationToken);
        }

        public async Task<IEnumerable<Wallet>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Wallet>()
        .Include(w => w.User)
        .Where(w => w.UserId == userId && w.IsActive)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Wallet wallet,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<Wallet>()
                .AddAsync(wallet, cancellationToken);
        }

        public void Remove(Wallet wallet)
        {
            _context.Set<Wallet>().Remove(wallet);
        }

        public async Task<List<Wallet>?> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Wallet>()
                .Include(w => w.User)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
