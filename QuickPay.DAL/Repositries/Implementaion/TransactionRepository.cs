using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly QuickPayDbContext _context;

        public TransactionRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.Transactions.AddAsync(
                transaction, cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(
            int accountId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(t =>
                    t.FromAccountId == accountId ||
                    t.ToAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetHistoryForUserAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var ownedWalletIds = _context.Set<Wallet>()
                .Where(w => w.UserId == userId)
                .Select(w => w.Id);

            var ownedSharedWalletIds = _context.Set<SharedWallet>()
                .Where(sw => sw.Members.Any(m => m.UserId == userId))
                .Select(sw => sw.Id);

            var accountIds = ownedWalletIds.Concat(ownedSharedWalletIds);

            return await _context.Transactions
                .Where(t =>
                    accountIds.Contains(t.FromAccountId) ||
                    accountIds.Contains(t.ToAccountId))
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
