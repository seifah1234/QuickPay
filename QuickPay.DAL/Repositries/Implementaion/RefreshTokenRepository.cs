using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly QuickPayDbContext _context;

        public RefreshTokenRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.Token == token,
                    cancellationToken);
        }

        public async Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens.AddAsync(
                refreshToken,
                cancellationToken);
        }
    }
}
