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

        public async Task<IEnumerable<FinancialAccount>> GetMyAccountsAsync(
    int userId,
    CancellationToken cancellationToken = default)
        {
            var wallets = await _context.Set<Wallet>()
                .Include(w => w.User)
                .Where(w => w.IsActive && w.UserId == userId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var sharedWallets = await _context.Set<SharedWallet>()
                .Where(sw =>
                    sw.IsActive &&
                    (sw.Members.Any(m => m.UserId == userId)))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = wallets
                .Cast<FinancialAccount>()
                .Concat(sharedWallets.Cast<FinancialAccount>())
                .ToList();

            return result;
        }
        public async Task<IEnumerable<FinancialAccount>> SearchRecipientAccountsAsync(
    string query,
    int excludeUserId,
    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<FinancialAccount>();
            }

            var matchingWallets = await _context.Set<Wallet>()
                .Include(w => w.User)
                .Where(w =>
                    w.IsActive &&
                    w.UserId != excludeUserId &&
                    (w.User.UserName.Contains(query) ||
                     w.User.PhoneNumber.Contains(query)))
                .AsNoTracking()
                .Take(10)
                .ToListAsync(cancellationToken);

            var matchingSharedWallets = await _context.Set<SharedWallet>()
                .Where(sw =>
                    sw.IsActive &&
                    sw.Name.Contains(query))
                .AsNoTracking()
                .Take(10)
                .ToListAsync(cancellationToken);

            var result = matchingWallets
                .Cast<FinancialAccount>()
                .Concat(matchingSharedWallets.Cast<FinancialAccount>())
                .Take(10)
                .ToList();

            return result;
        }

        public async Task<bool> IsUserAuthorizedForAccountAsync(
            int accountId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var isOwnWallet = await _context.Set<Wallet>()
                .AnyAsync(
                    w => w.Id == accountId && w.UserId == userId,
                    cancellationToken);

            if (isOwnWallet)
            {
                return true;
            }

            var isSharedWalletOwnerOrMember = await _context.Set<SharedWallet>()
                .AnyAsync(
                    sw => sw.Id == accountId &&
                          (sw.Members.Any(m => m.UserId == userId)),
                    cancellationToken);

            return isSharedWalletOwnerOrMember;
        }
    }
}
