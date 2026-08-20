using QuickPay.BLL.DTOs;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface ITransactionHistoryService
    {
        Task<IEnumerable<TransactionHistoryDto>> GetUserHistoryAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
