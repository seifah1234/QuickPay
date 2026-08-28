using QuickPay.BLL.DTOs.Auth;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResultDto> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken cancellationToken = default);

        Task<AuthResultDto> VerifyPhoneAsync(
            VerifyOtpRequestDto request,
            CancellationToken cancellationToken = default);

        Task<AuthResultDto> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken = default);

        Task<AuthResultDto> RefreshAsync(
            RefreshTokenRequestDto request,
            CancellationToken cancellationToken = default);

        Task LogoutAsync(
            string refreshToken,
            CancellationToken cancellationToken = default);
        
        Task<AuthResultDto> ExternalLoginAsync(ExternalLoginRequestDto? request,
            CancellationToken cancellationToken = default);

        Task<AuthResultDto> AddPhoneNumberAsync(
            int userId,
            string phoneNumber,
            CancellationToken cancellationToken = default);
    }
}
