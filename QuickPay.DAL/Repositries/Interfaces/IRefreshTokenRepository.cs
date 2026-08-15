using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default);
    }
}
