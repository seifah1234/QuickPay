using QuickPay.BLL.DTOs;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IWalletService
    {
        Task<IEnumerable<WalletDto>> GetMyWalletsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<WalletDto> CreateWalletAsync(
            CreateWalletDto request,
            CancellationToken cancellationToken = default);

        Task<WalletDto> RenameWalletAsync(
            RenameWalletDto request,
            CancellationToken cancellationToken = default);

        Task DeleteWalletAsync(
            int walletId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<WalletDto> DepositAsync(
            DepositWithdrawRequestDto request,
            CancellationToken cancellationToken = default);

        Task<WalletDto> WithdrawAsync(
            DepositWithdrawRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
