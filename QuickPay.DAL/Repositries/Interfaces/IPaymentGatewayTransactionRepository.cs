using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IPaymentGatewayTransactionRepository
    {
        Task AddAsync(
            PaymentGatewayTransaction transaction,
            CancellationToken cancellationToken = default);

        Task<PaymentGatewayTransaction?> GetByGatewayTransactionIdAsync(
            string gatewayTransactionId,
            CancellationToken cancellationToken = default);

        Task<PaymentGatewayTransaction?> GetByProviderOrderIdAsync(
            string providerOrderId,
            CancellationToken cancellationToken = default);

        Task<PaymentGatewayTransaction?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Successful deposits made through this BankAccount that still
        /// have some un-refunded Amount left, oldest first - the pool
        /// Withdraw draws from since there's no separate payout API.
        /// </summary>
        Task<IEnumerable<PaymentGatewayTransaction>> GetRefundableDepositsAsync(
            int bankAccountId,
            CancellationToken cancellationToken = default);
    }
}
