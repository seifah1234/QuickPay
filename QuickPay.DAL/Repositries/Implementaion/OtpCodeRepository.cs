using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class OtpCodeRepository : IOtpCodeRepository
    {
        private readonly QuickPayDbContext _context;

        public OtpCodeRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<OtpCode?> GetLatestActiveAsync(
            int userId,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            return await _context.OtpCodes
                .Where(x =>
                    x.UserId == userId &&
                    x.Purpose == purpose &&
                    !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            OtpCode otpCode,
            CancellationToken cancellationToken = default)
        {
            await _context.OtpCodes.AddAsync(otpCode, cancellationToken);
        }
    }
}
