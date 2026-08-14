using QuickPay.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Abstraction
{
    public interface ITransactionHistoryService
    {
        Task<List<TransactionHistoryDto>> GetUserHistoryAsync(int userId, int pageNumber, int pageSize);
    }
}

