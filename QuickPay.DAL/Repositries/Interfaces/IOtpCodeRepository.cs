using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IOtpCodeRepository
    {
        Task<OtpCode?> GetLatestActiveAsync(
            int userId,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            OtpCode otpCode,
            CancellationToken cancellationToken = default);
    }
}
