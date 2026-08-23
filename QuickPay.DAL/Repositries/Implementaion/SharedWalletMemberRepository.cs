using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class SharedWalletMemberRepository : ISharedWalletMemberRepository
    {
        private readonly QuickPayDbContext _context;

        public SharedWalletMemberRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<SharedWalletMember?> GetAsync(
            int sharedWalletId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SharedWalletMember>()
                .FirstOrDefaultAsync(
                    m => m.SharedWalletId == sharedWalletId &&
                         m.UserId == userId,
                    cancellationToken);
        }

        public async Task<int> CountAdminsAsync(
            int sharedWalletId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SharedWalletMember>()
                .CountAsync(
                    m => m.SharedWalletId == sharedWalletId &&
                         m.Role == SharedWalletRole.Admin,
                    cancellationToken);
        }

        public async Task AddAsync(
            SharedWalletMember member,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<SharedWalletMember>()
                .AddAsync(member, cancellationToken);
        }

        public void Remove(SharedWalletMember member)
        {
            _context.Set<SharedWalletMember>().Remove(member);
        }
    }
}
