using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);

        Task AddAsync(Transaction transaction);

        Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId);
    }
}
