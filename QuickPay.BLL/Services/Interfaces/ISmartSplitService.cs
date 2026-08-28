using QuickPay.BLL.DTOs.SmartSplit;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface ISmartSplitService
    {
        Task<IEnumerable<SplitGroupDto>> GetMySplitsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<SplitGroupDto> GetDetailsAsync(
            int splitGroupId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<SplitGroupDto> CreateSplitAsync(
            CreateSplitGroupDto request,
            CancellationToken cancellationToken = default);

        Task<SplitGroupDto> PayShareAsync(
            PaySplitShareDto request,
            CancellationToken cancellationToken = default);

        Task SendReminderAsync(
            SendSplitReminderDto request,
            CancellationToken cancellationToken = default);

        Task CancelAsync(
            CancelSplitGroupDto request,
            CancellationToken cancellationToken = default);
    }
}
