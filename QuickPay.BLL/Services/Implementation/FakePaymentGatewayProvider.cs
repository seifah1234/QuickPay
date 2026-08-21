using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuickPay.BLL.Services.Implementation
{
    public class FakePaymentGatewayProvider : IPaymentGatewayProvider
    {
        private const string DevSecret = "dev-only-fake-gateway-secret";

        public Task<GatewayChargeResult> InitiateChargeAsync(
            GatewayChargeRequest request,
            CancellationToken cancellationToken = default)
        {
            var gatewayTransactionId = request.MerchantReference;

            var result = new GatewayChargeResult
            {
                IsSuccess = true,
                GatewayTransactionId = gatewayTransactionId,
                CheckoutUrl = $"/PaymentGateway/FakeCheckout/{gatewayTransactionId}?amount={request.Amount}"
            };

            return Task.FromResult(result);
        }

        public Task<GatewayPayoutResult> InitiatePayoutAsync(
            GatewayPayoutRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = new GatewayPayoutResult
            {
                IsSuccess = true,
                GatewayTransactionId = $"FAKE-{Guid.NewGuid():N}"
            };

            return Task.FromResult(result);
        }

        public Task<GatewayRefundResult> RefundAsync(
            GatewayRefundRequest request,
            CancellationToken cancellationToken = default)
        {
            // Simulated gateway - nothing was really charged, so "refunding"
            // always succeeds instantly.
            return Task.FromResult(new GatewayRefundResult { IsSuccess = true });
        }

        public bool VerifyWebhookSignature(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature)
        {
            var expected = ComputeSignature(rawBody);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(receivedSignature));
        }

        public GatewayWebhookEvent ParseWebhookEvent(
            string rawBody,
            IDictionary<string, string> query)
        {
            var payload = JsonSerializer.Deserialize<FakeWebhookPayload>(rawBody)
                ?? throw new InvalidOperationException(
                    "Malformed fake webhook payload.");

            return new GatewayWebhookEvent
            {
                EventType = GatewayWebhookEventType.Transaction,
                GatewayTransactionId = payload.GatewayTransactionId,
                ProviderTransactionId = payload.GatewayTransactionId,
                ProviderOrderId = payload.GatewayTransactionId,
                IsSuccessful = payload.IsSuccessful,
                Amount = payload.Amount,
                // The fake gateway simulates the whole flow in one webhook
                // call (no separate TOKEN callback), so card fields are
                // attached directly to this Transaction event. The service
                // layer knows to complete LinkCard immediately when it sees
                // these already populated.
                CardToken = payload.IsSuccessful
                    ? $"FAKE-CARD-TOKEN-{payload.GatewayTransactionId}"
                    : null,
                MaskedPan = payload.IsSuccessful ? "4242" : null,
                CardSubType = payload.IsSuccessful ? "Visa" : null
            };
        }

        public static string ComputeSignature(string rawBody)
        {
            var keyBytes = Encoding.UTF8.GetBytes(DevSecret);
            var bodyBytes = Encoding.UTF8.GetBytes(rawBody);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(bodyBytes);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private class FakeWebhookPayload
        {
            public string GatewayTransactionId { get; set; } = string.Empty;

            public bool IsSuccessful { get; set; }

            public decimal Amount { get; set; }
        }
    }
}
