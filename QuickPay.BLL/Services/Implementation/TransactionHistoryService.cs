using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class TransactionHistoryService : ITransactionHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TransactionHistoryDto>> GetUserHistoryAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var myAccounts = await _unitOfWork.FinancialAccounts
                .GetMyAccountsAsync(userId, cancellationToken);

            var myAccountIds = myAccounts
                .Select(a => a.Id)
                .ToHashSet();

            var transactions = await _unitOfWork.Transactions
                .GetHistoryForUserAsync(
                    userId, pageNumber, pageSize, cancellationToken);

            return transactions.Select(t => new TransactionHistoryDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                Direction = myAccountIds.Contains(t.FromAccountId)
                    ? "Outgoing"
                    : "Incoming"
            });
        }
    }
}
