using QuickPay.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repo.Abstraction
{
    public interface ITransactionHistoryRepo
    {
        Task<List<Transaction>> GetByUserIdAsync(int userId, int pageNumber, int pageSize);
    }
}
