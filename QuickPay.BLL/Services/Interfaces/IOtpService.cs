using QuickPay.DAL.Enums;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IOtpService
    {
        Task GenerateAndSendAsync(
            int userId,
            string phoneNumber,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default);

        Task<bool> VerifyAsync(
            int userId,
            string code,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default);
    }
}
