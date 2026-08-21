using Microsoft.Extensions.Options;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.Settings;
using System.Net.Http.Headers;
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
                    // Informational only - the service correlates webhooks
                    // using its own MerchantReference (special_reference),
                    // not this id, since it's echoed back reliably as
                    // obj.order.merchant_order_id in the callback.
                    GatewayTransactionId = body.Id ?? request.MerchantReference,
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

        public async Task<GatewayRefundResult> RefundAsync(
            GatewayRefundRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var amountCents = (int)(request.Amount * 100);

                // NOTE: unconfirmed against a live sandbox response - this
                // reuses Paymob's classic void/refund endpoint with the
                // Secret Key as bearer auth (same auth style the Intention
                // API uses elsewhere in this class). Verify with a real
                // test transaction before depending on this in production;
                // Paymob may require a different refund route for
                // Intention-API transactions specifically.
                var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{_settings.BaseUrl}/api/acceptance/void_refund/refund")
                {
                    Content = JsonContent.Create(new
                    {
                        transaction_id = request.GatewayTransactionId,
                        amount_cents = amountCents
                    })
                };

                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Token", _settings.SecretKey);

                var response = await _httpClient.SendAsync(
                    httpRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content
                        .ReadAsStringAsync(cancellationToken);

                    return new GatewayRefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Paymob refund failed ({(int)response.StatusCode}): {errorBody}"
                    };
                }

                return new GatewayRefundResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new GatewayRefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public bool VerifyWebhookSignature(
            string rawBody,
            IDictionary<string, string> query,
            string receivedSignature)
        {
            if (string.IsNullOrEmpty(receivedSignature))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                if (!root.TryGetProperty("obj", out var obj))
                {
                    return false;
                }

                var type = root.TryGetProperty("type", out var typeEl)
                    ? typeEl.GetString()
                    : null;

                var concatenated = type == "TOKEN"
                    ? BuildTokenHmacString(obj)
                    : BuildTransactionHmacString(obj);

                var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
                var messageBytes = Encoding.UTF8.GetBytes(concatenated);

                using var hmac = new HMACSHA512(keyBytes);
                var hash = hmac.ComputeHash(messageBytes);
                var expectedSignature = Convert.ToHexString(hash).ToLowerInvariant();

                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSignature),
                    Encoding.UTF8.GetBytes(receivedSignature.ToLowerInvariant()));
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public GatewayWebhookEvent ParseWebhookEvent(
            string rawBody,
            IDictionary<string, string> query)
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("obj", out var obj))
            {
                throw new InvalidOperationException(
                    "Malformed Paymob webhook payload - missing 'obj'.");
            }

            var type = root.TryGetProperty("type", out var typeEl)
                ? typeEl.GetString()
                : null;

            if (type == "TOKEN")
            {
                // Card-saving callback - fired separately from the payment
                // callback, only when the integration has "Save Card"
                // enabled. This is the ONLY place the reusable card token,
                // masked PAN and card brand actually show up.
                return new GatewayWebhookEvent
                {
                    EventType = GatewayWebhookEventType.CardToken,
                    IsSuccessful = true,
                    ProviderOrderId = GetRawOrNull(obj, "order_id"),
                    CardToken = GetRawOrNull(obj, "token"),
                    MaskedPan = GetRawOrNull(obj, "masked_pan"),
                    CardSubType = GetRawOrNull(obj, "card_subtype")
                };
            }

            // Default: TRANSACTION callback (payment success/failure).
            var merchantReference =
                GetNestedRawOrNull(obj, "order", "merchant_order_id") ??
                string.Empty;

            var isSuccessful =
                obj.TryGetProperty("success", out var successEl) &&
                successEl.ValueKind == JsonValueKind.True;

            var amountCents = obj.TryGetProperty("amount_cents", out var amountEl) &&
                amountEl.TryGetInt64(out var cents)
                ? cents
                : 0L;

            return new GatewayWebhookEvent
            {
                EventType = GatewayWebhookEventType.Transaction,
                GatewayTransactionId = merchantReference,
                ProviderOrderId = GetNestedRawOrNull(obj, "order", "id"),
                ProviderTransactionId = GetRawOrNull(obj, "id"),
                IsSuccessful = isSuccessful,
                Amount = amountCents / 100m
            };
        }

        // Field order is fixed by Paymob's docs - do not reorder.
        // https://developers.paymob.com/paymob-docs/developers/webhook-callbacks-and-hmac/hmac
        private static string BuildTransactionHmacString(JsonElement obj)
        {
            return string.Concat(
                GetRaw(obj, "amount_cents"),
                GetRaw(obj, "created_at"),
                GetRaw(obj, "currency"),
                GetRaw(obj, "error_occured"),
                GetRaw(obj, "has_parent_transaction"),
                GetRaw(obj, "id"),
                GetRaw(obj, "integration_id"),
                GetRaw(obj, "is_3d_secure"),
                GetRaw(obj, "is_auth"),
                GetRaw(obj, "is_capture"),
                GetRaw(obj, "is_refunded"),
                GetRaw(obj, "is_standalone_payment"),
                GetRaw(obj, "is_voided"),
                GetNestedRaw(obj, "order", "id"),
                GetRaw(obj, "owner"),
                GetRaw(obj, "pending"),
                GetNestedRaw(obj, "source_data", "pan"),
                GetNestedRaw(obj, "source_data", "sub_type"),
                GetNestedRaw(obj, "source_data", "type"),
                GetRaw(obj, "success"));
        }

        // Card-token (TOKEN) callback fields, taken in lexicographical
        // (alphabetical) key order per Paymob's docs.
        private static string BuildTokenHmacString(JsonElement obj)
        {
            return string.Concat(
                GetRaw(obj, "card_subtype"),
                GetRaw(obj, "created_at"),
                GetRaw(obj, "email"),
                GetRaw(obj, "id"),
                GetRaw(obj, "masked_pan"),
                GetRaw(obj, "merchant_id"),
                GetRaw(obj, "order_id"),
                GetRaw(obj, "token"));
        }

        private static string GetRaw(JsonElement obj, string propertyName)
        {
            return GetRawOrNull(obj, propertyName) ?? string.Empty;
        }

        private static string? GetRawOrNull(JsonElement obj, string propertyName)
        {
            if (!obj.TryGetProperty(propertyName, out var el))
            {
                return null;
            }

            return el.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                JsonValueKind.String => el.GetString(),
                JsonValueKind.Number => el.GetRawText(),
                _ => el.GetRawText()
            };
        }

        private static string GetNestedRaw(
            JsonElement obj, string parentProperty, string childProperty)
        {
            return GetNestedRawOrNull(obj, parentProperty, childProperty)
                ?? string.Empty;
        }

        private static string? GetNestedRawOrNull(
            JsonElement obj, string parentProperty, string childProperty)
        {
            if (!obj.TryGetProperty(parentProperty, out var parent))
            {
                return null;
            }

            return GetRawOrNull(parent, childProperty);
        }

        private class IntentionResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("client_secret")]
            public string? ClientSecret { get; set; }
        }
    }
}
