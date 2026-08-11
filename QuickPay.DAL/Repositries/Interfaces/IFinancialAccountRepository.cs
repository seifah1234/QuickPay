using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IFinancialAccountRepository
    {
        Task<FinancialAccount?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<FinancialAccount>> GetActiveAsync(
            CancellationToken cancellationToken = default);
    }
}
