using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Abstraction;
using QuickPay.DAL.Repo.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Implementation
{
    public class TransactionHistoryService:ITransactionHistoryService
    {
        private readonly ITransactionHistoryRepo _historyRepo;

        public TransactionHistoryService(ITransactionHistoryRepo historyRepo)
        {
            _historyRepo = historyRepo;
        }

        public async Task<List<TransactionHistoryDto>> GetUserHistoryAsync(int userId, int pageNumber, int pageSize)
        {
            var transactions = await _historyRepo.GetByUserIdAsync(userId, pageNumber, pageSize);

            return transactions.Select(t => new TransactionHistoryDto
            {
                TransactionId = t.TransactionId,
                Type = t.Type,
                Amount = t.Amount,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            }).ToList();
        }
    }
}
