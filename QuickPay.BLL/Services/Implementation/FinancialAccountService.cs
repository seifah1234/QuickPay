using AutoMapper;
using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Implementation
{

    public class FinancialAccountService : IFinancialAccountService
    {
        private readonly IFinancialAccountRepository _repository;
        private readonly IMapper _mapper;

        public FinancialAccountService(
            IFinancialAccountRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AccountDto>> GetAvailableAccountsAsync(
    CancellationToken cancellationToken = default)
        {
            var accounts = await _repository.GetActiveAsync(
                cancellationToken);

            return accounts.Select(account => new AccountDto
            {
                Id = account.Id,
                Balance = account.Balance,
                Currency = account.Currency,
                IsActive = account.IsActive,
                DisplayName = GetDisplayName(account)
            });
        }

        private static string GetDisplayName(FinancialAccount account)
        {
            return account switch
            {
                Wallet wallet =>
                    $"{wallet.User?.UserName ?? "Unknown"} Wallet",

                SharedWallet sharedWallet =>
                    sharedWallet.Name,

                _ =>
                    $"Account #{account.Id}"
            };
        }
    }
}
