using QuickPay.DAL.Repositries.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }

        IRefreshTokenRepository RefreshTokens { get; }

        IOtpCodeRepository OtpCodes { get; }

        ITransactionRepository Transactions { get; }

        IFinancialAccountRepository FinancialAccounts { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(
            CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(
            CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(
            CancellationToken cancellationToken = default);
    }
}
