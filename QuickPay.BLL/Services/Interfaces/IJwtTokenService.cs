using QuickPay.DAL.Entities;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

        string GenerateRefreshToken();
    }
}
