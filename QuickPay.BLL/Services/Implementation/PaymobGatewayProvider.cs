using Microsoft.Extensions.Options;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.Settings;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickPay.BLL.Services.Implementation
{
    public class PaymobGatewayProvider : IPaymentGatewayProvider
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobSettings _settings;

        public PaymobGatewayProvider(
            HttpClient httpClient,
            IOptions<PaymobSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<GatewayChargeResult> InitiateChargeAsync(
            GatewayChargeRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var authToken = await GetAuthTokenAsync(cancellationToken);

                var amountCents = (int)(request.Amount * 100);

                var orderId = await CreateOrderAsync(
                    authToken, amountCents, request.MerchantReference, cancellationToken);

                var paymentKey = await GetPaymentKeyAsync(
                    authToken, orderId, amountCents, request, cancellationToken);

                return new GatewayChargeResult
                {
                    IsSuccess = true,
                    GatewayTransactionId = orderId.ToString(),
                    CheckoutUrl =
                        $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentKey}"
                };
            }
            catch (Exception ex)
            {
                return new GatewayChargeResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public Task<GatewayPayoutResult> InitiatePayoutAsync(
            GatewayPayoutRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(
                "Paymob payouts require Disbursement API access - not yet configured. See PATCH_NOTES.");
        }

        public bool VerifyWebhookSignature(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature)
        {
            throw new NotImplementedException(
                "Fill in Paymob's documented HMAC field concatenation here before going live - see developers.paymob.com webhook docs.");
        }

        public GatewayWebhookEvent ParseWebhookEvent(
            string rawBody,
            IDictionary<string, string> query)
        {
            var payload = JsonSerializer.Deserialize<PaymobWebhookPayload>(rawBody)
                ?? throw new InvalidOperationException(
                    "Malformed Paymob webhook payload.");

            return new GatewayWebhookEvent
            {
                GatewayTransactionId = payload.Order?.Id.ToString() ?? string.Empty,
                IsSuccessful = payload.Success,
                Amount = payload.AmountCents / 100m
            };
        }

        private async Task<string> GetAuthTokenAsync(
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/auth/tokens",
                new { api_key = _settings.ApiKey },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content
                .ReadFromJsonAsync<AuthTokenResponse>(
                    cancellationToken: cancellationToken);

            return body?.Token
                ?? throw new InvalidOperationException(
                    "Paymob auth response had no token.");
        }

        private async Task<long> CreateOrderAsync(
            string authToken,
            int amountCents,
            string merchantReference,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/ecommerce/orders",
                new
                {
                    auth_token = authToken,
                    delivery_needed = false,
                    amount_cents = amountCents,
                    currency = "EGP",
                    merchant_order_id = merchantReference,
                    items = Array.Empty<object>()
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content
                .ReadFromJsonAsync<OrderResponse>(
                    cancellationToken: cancellationToken);

            return body?.Id
                ?? throw new InvalidOperationException(
                    "Paymob order response had no id.");
        }

        private async Task<string> GetPaymentKeyAsync(
            string authToken,
            long orderId,
            int amountCents,
            GatewayChargeRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/acceptance/payment_keys",
                new
                {
                    auth_token = authToken,
                    amount_cents = amountCents,
                    expiration = 3600,
                    order_id = orderId,
                    currency = "EGP",
                    integration_id = _settings.IntegrationId,
                    billing_data = new
                    {
                        first_name = request.PayerFullName,
                        last_name = "N/A",
                        email = request.PayerEmail,
                        phone_number = request.PayerPhoneNumber,
                        apartment = "NA",
                        floor = "NA",
                        street = "NA",
                        building = "NA",
                        city = "NA",
                        country = "EG",
                        state = "NA"
                    }
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content
                .ReadFromJsonAsync<PaymentKeyResponse>(
                    cancellationToken: cancellationToken);

            return body?.Token
                ?? throw new InvalidOperationException(
                    "Paymob payment key response had no token.");
        }

        private class AuthTokenResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;
        }

        private class OrderResponse
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
        }

        private class PaymentKeyResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;
        }

        private class PaymobWebhookPayload
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("amount_cents")]
            public int AmountCents { get; set; }

            [JsonPropertyName("order")]
            public PaymobOrderRef? Order { get; set; }
        }

        private class PaymobOrderRef
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
        }
    }
}
