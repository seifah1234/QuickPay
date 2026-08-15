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

        public async Task<IEnumerable<AccountDto>> GetMyAccountsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var accounts = await _repository.GetMyAccountsAsync(
                userId,
                cancellationToken);

            return accounts.Select(ToAccountDto);
        }

        public async Task<IEnumerable<AccountDto>> SearchRecipientAccountsAsync(
            string query,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var accounts = await _repository.SearchRecipientAccountsAsync(
                query,
                currentUserId,
                cancellationToken);

            return accounts.Select(ToAccountDto);
        }

        private static AccountDto ToAccountDto(FinancialAccount account)
        {
            return new AccountDto
            {
                Id = account.Id,
                Balance = account.Balance,
                Currency = account.Currency,
                IsActive = account.IsActive,
                DisplayName = GetDisplayName(account)
            };
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
