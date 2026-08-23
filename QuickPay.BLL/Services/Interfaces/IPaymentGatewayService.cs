using QuickPay.BLL.DTOs.PaymentGateway;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<GatewayInitiationResultDto> InitiateDepositAsync(
            InitiateDepositRequestDto request,
            CancellationToken cancellationToken = default);

        Task<GatewayInitiationResultDto> InitiateWithdrawAsync(
            InitiateWithdrawRequestDto request,
            CancellationToken cancellationToken = default);

        Task<GatewayInitiationResultDto> InitiateLinkCardAsync(
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<bool> HandleWebhookAsync(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature,
            CancellationToken cancellationToken = default);
    }
}
