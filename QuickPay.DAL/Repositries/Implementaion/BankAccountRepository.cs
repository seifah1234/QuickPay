using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly QuickPayDbContext _context;

        public BankAccountRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            BankAccount bankAccount,
            CancellationToken cancellationToken = default)
        {
            await _context.BankAccounts.AddAsync(
                bankAccount, cancellationToken);
        }

        public async Task<BankAccount?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.BankAccounts
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<BankAccount>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.BankAccounts
                .Where(x => x.UserId == userId && x.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
