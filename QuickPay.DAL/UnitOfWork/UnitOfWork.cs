using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.UnitOfWork
{
    using Microsoft.EntityFrameworkCore.Storage;
    using QuickPay.DAL.Repositries.Interfaces;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuickPayDbContext _context;

        public IUserRepository Users { get; }

        public IRefreshTokenRepository RefreshTokens { get; }

        public IOtpCodeRepository OtpCodes { get; }

        public INotificationRepository Notifications { get; }
        private IDbContextTransaction? _transaction;

        public ITransactionRepository Transactions { get; }

        public IFinancialAccountRepository FinancialAccounts { get; }

        public UnitOfWork(
            QuickPayDbContext context,
            ITransactionRepository transactionRepository,
            IFinancialAccountRepository financialAccountRepository,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IOtpCodeRepository otpCodeRepository,
            INotificationRepository notificationRepository)
        {
            _context = context;

            Transactions = transactionRepository;
            FinancialAccounts = financialAccountRepository;

            Users = userRepository;
            RefreshTokens = refreshTokenRepository;
            OtpCodes = otpCodeRepository;
            Notifications = notificationRepository;
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database
                .BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return;

            await _transaction.CommitAsync(cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task RollbackTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return;

            await _transaction.RollbackAsync(cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }
    }
}
