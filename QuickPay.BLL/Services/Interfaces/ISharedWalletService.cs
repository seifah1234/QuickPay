using QuickPay.BLL.DTOs;
using QuickPay.BLL.DTOs.SharedWallet;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface ISharedWalletService
    {
        Task<IEnumerable<SharedWalletDto>> GetMySharedWalletsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<SharedWalletDto> GetDetailsAsync(
            int sharedWalletId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TransactionHistoryDto>> GetHistoryAsync(
            int sharedWalletId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<SharedWalletDto> CreateAsync(
            CreateSharedWalletDto request,
            CancellationToken cancellationToken = default);

        Task<SharedWalletDto> RenameAsync(
            RenameSharedWalletDto request,
            CancellationToken cancellationToken = default);

        Task<SharedWalletDto> AddMemberAsync(
            AddSharedWalletMemberDto request,
            CancellationToken cancellationToken = default);

        Task RemoveMemberAsync(
            RemoveSharedWalletMemberDto request,
            CancellationToken cancellationToken = default);

        Task CloseAsync(
            CloseSharedWalletDto request,
            CancellationToken cancellationToken = default);
    }
}
