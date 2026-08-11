using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class FinancialAccountRepository : IFinancialAccountRepository
    {
        private readonly QuickPayDbContext _context;

        public FinancialAccountRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialAccount?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.FinancialAccounts
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<IEnumerable<FinancialAccount>> GetActiveAsync(
    CancellationToken cancellationToken = default)
        {
            return await _context.FinancialAccounts
                .Include(x => ((Wallet)x).User)
                .Include(x => ((SharedWallet)x).Owner)
                .Where(x => x.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
