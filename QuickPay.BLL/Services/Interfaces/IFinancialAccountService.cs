using QuickPay.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IFinancialAccountService
    {
        Task<IEnumerable<AccountDto>> GetAvailableAccountsAsync(
            CancellationToken cancellationToken = default);
    }
}
