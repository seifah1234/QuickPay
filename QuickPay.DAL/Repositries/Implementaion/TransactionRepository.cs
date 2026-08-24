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
    string? filterBy = null,
    string? filterValue = null,
    CancellationToken cancellationToken = default)
        {
            var ownedWalletIds = _context.Set<Wallet>()
                .Where(w => w.UserId == userId)
                .Select(w => w.Id);

            var ownedSharedWalletIds = _context.Set<SharedWallet>()
                .Where(sw => sw.Members.Any(m => m.UserId == userId))
                .Select(sw => sw.Id);

            var accountIds = ownedWalletIds.Concat(ownedSharedWalletIds);

            var query = _context.Transactions
                .Where(t =>
                    accountIds.Contains(t.FromAccountId) ||
                    accountIds.Contains(t.ToAccountId))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterBy) &&
                !string.IsNullOrWhiteSpace(filterValue))
            {
                switch (filterBy)
                {
                    case "type":
                        if (Enum.TryParse<Enums.TransactionType>(
                                filterValue, true, out var typeValue))
                        {
                            query = query.Where(t => t.Type == typeValue);
                        }
                        break;

                    case "status":
                        if (Enum.TryParse<Enums.TransactionStatus>(
                                filterValue, true, out var statusValue))
                        {
                            query = query.Where(t => t.Status == statusValue);
                        }
                        break;

                    case "search":
                        query = query.Where(t =>
                            t.Type.ToString().Contains(filterValue) ||
                            t.Status.ToString().Contains(filterValue));
                        break;

                    case "date":
                        var dates = filterValue.Split('|');

                        if (dates.Length == 2 &&
                            DateTime.TryParse(dates[0], out var fromDate) &&
                            DateTime.TryParse(dates[1], out var toDate))
                        {
                            fromDate = fromDate.Date;
                            toDate = toDate.Date.AddDays(1);

                            query = query.Where(t =>
                                t.CreatedAt >= fromDate &&
                                t.CreatedAt < toDate);
                        }
                        break;
                }
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Transaction>?> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
