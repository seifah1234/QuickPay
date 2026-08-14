using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Database;
using QuickPay.DAL.Entity;
using QuickPay.DAL.Repo.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repo.Implementation
{
    public class TransactionHistoryRepo:ITransactionHistoryRepo
    {
        private readonly ProjectDBContext _context;

        public TransactionHistoryRepo(ProjectDBContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetByUserIdAsync(int userId, int pageNumber, int pageSize)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
