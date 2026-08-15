using QuickPay.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IFinancialAccountService
    {
        /// <summary>Accounts the given user can transfer FROM.</summary>
        Task<IEnumerable<AccountDto>> GetMyAccountsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AccountDto>> SearchRecipientAccountsAsync(
            string query,
            int currentUserId,
            CancellationToken cancellationToken = default);
    }
}
