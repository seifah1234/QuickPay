using QuickPay.BLL.DTOs.PaymentGateway;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IPaymentGatewayProvider
    {
        Task<GatewayChargeResult> InitiateChargeAsync(
            GatewayChargeRequest request,
            CancellationToken cancellationToken = default);

        Task<GatewayPayoutResult> InitiatePayoutAsync(
            GatewayPayoutRequest request,
            CancellationToken cancellationToken = default);


        bool VerifyWebhookSignature(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature);

        GatewayWebhookEvent ParseWebhookEvent(
            string rawBody,
            IDictionary<string, string> query);
    }
}
