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


        Task<IEnumerable<FinancialAccount>> GetMyAccountsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<FinancialAccount>> SearchRecipientAccountsAsync(
            string query,
            int excludeUserId,
            CancellationToken cancellationToken = default);

  
        Task<bool> IsUserAuthorizedForAccountAsync(
            int accountId,
            int userId,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<FinancialAccount>> GetAllAsync(CancellationToken cancellationToken);
    }
}
