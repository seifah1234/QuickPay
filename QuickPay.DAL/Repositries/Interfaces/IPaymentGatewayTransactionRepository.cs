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

        Task<decimal> GetPendingWithdrawAmountAsync(
            int walletId,
            CancellationToken cancellationToken = default);
    }
}
