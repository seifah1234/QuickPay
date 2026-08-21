using Microsoft.Extensions.Options;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.Settings;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
                var amountCents = (int)(request.Amount * 100);

                var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post, $"{_settings.BaseUrl}/v1/intention/")
                {
                    Content = JsonContent.Create(new
                    {
                        amount = amountCents,
                        currency = "EGP",
                        payment_methods = new[] { _settings.IntegrationId },
                        special_reference = request.MerchantReference,
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
                    })
                };

                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Token", _settings.SecretKey);

                var response = await _httpClient.SendAsync(
                    httpRequest, cancellationToken);
                response.EnsureSuccessStatusCode();

                var body = await response.Content
                    .ReadFromJsonAsync<IntentionResponse>(
                        cancellationToken: cancellationToken);

                if (body?.ClientSecret is null)
                {
                    throw new InvalidOperationException(
                        "Paymob intention response had no client_secret.");
                }

                return new GatewayChargeResult
                {
                    IsSuccess = true,
                    GatewayTransactionId = body.Id ?? body.ClientSecret,
                    CheckoutUrl =
                        $"{_settings.BaseUrl}/unifiedcheckout/?publicKey={_settings.PublicKey}&clientSecret={body.ClientSecret}"
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
                "Withdraw is currently disabled - see PATCH_NOTES.");
        }

        public bool VerifyWebhookSignature(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature)
        {
            throw new NotImplementedException(
                "Fill in Paymob's current HMAC field concatenation for Intention API callbacks before re-enabling this.");
        }

        public GatewayWebhookEvent ParseWebhookEvent(
            string rawBody,
            IDictionary<string, string> query)
        {
            var payload = JsonSerializer.Deserialize<IntentionWebhookPayload>(rawBody)
                ?? throw new InvalidOperationException(
                    "Malformed Paymob webhook payload.");

            return new GatewayWebhookEvent
            {
                GatewayTransactionId = payload.Obj?.Id?.ToString() ?? string.Empty,
                IsSuccessful = payload.Obj?.Success ?? false,
                Amount = (payload.Obj?.AmountCents ?? 0) / 100m
            };
        }

        private class IntentionResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("client_secret")]
            public string? ClientSecret { get; set; }
        }

        private class IntentionWebhookPayload
        {
            [JsonPropertyName("obj")]
            public IntentionWebhookObj? Obj { get; set; }
        }

        private class IntentionWebhookObj
        {
            [JsonPropertyName("id")]
            public long? Id { get; set; }

            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("amount_cents")]
            public int AmountCents { get; set; }
        }
    }
}
