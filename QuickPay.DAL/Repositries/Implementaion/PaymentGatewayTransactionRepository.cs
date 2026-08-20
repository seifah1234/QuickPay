using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class PaymentGatewayTransactionRepository
        : IPaymentGatewayTransactionRepository
    {
        private readonly QuickPayDbContext _context;

        public PaymentGatewayTransactionRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            PaymentGatewayTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.PaymentGatewayTransactions.AddAsync(
                transaction, cancellationToken);
        }

        public async Task<PaymentGatewayTransaction?> GetByGatewayTransactionIdAsync(
            string gatewayTransactionId,
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentGatewayTransactions
                .Include(x => x.Wallet)
                .FirstOrDefaultAsync(
                    x => x.GatewayTransactionId == gatewayTransactionId,
                    cancellationToken);
        }

        public async Task<PaymentGatewayTransaction?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentGatewayTransactions
                .Include(x => x.Wallet)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<decimal> GetPendingWithdrawAmountAsync(
            int walletId,
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentGatewayTransactions
                .Where(x =>
                    x.WalletId == walletId &&
                    x.Direction == Enums.PaymentGatewayDirection.Withdraw &&
                    x.Status == Enums.PaymentGatewayTransactionStatus.Pending)
                .SumAsync(x => x.Amount, cancellationToken);
        }
    }
}
