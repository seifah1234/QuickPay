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
            if (!string.IsNullOrWhiteSpace(request.SavedCardToken))
            {
                return await ChargeWithSavedTokenAsync(request, cancellationToken);
            }

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
                        notification_url = _settings.NotificationUrl,
                        save_card = true,
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

        /// <summary>
        /// Charges an existing saved card token directly - no new card
        /// entry, no redirect to Unified Checkout, using the classic
        /// (pre-Intention) Accept flow, which is the only flow Paymob
        /// documents for server-initiated token charges as of this
        /// writing: Auth token -> Order -> Payment Key -> Pay with
        /// source.subtype "TOKEN". The transaction result still arrives
        /// asynchronously via the normal TRANSACTION webhook, same as
        /// the Intention flow - this method only starts the charge, it
        /// does not itself confirm or touch any balance (that stays the
        /// webhook's job, for the same idempotency reasons as everywhere
        /// else in this file).
        ///
        /// If the issuing bank requires a 3-D Secure step-up even for a
        /// saved token, Paymob's pay response includes a redirect URL -
        /// handled below by surfacing it as CheckoutUrl so the caller
        /// can send the user there; otherwise CheckoutUrl is null and
        /// the deposit simply completes in the background.
        /// </summary>
        private async Task<GatewayChargeResult> ChargeWithSavedTokenAsync(
            GatewayChargeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var amountCents = (int)(request.Amount * 100);

                var authToken = await GetClassicAuthTokenAsync(cancellationToken);

                var orderId = await CreateClassicOrderAsync(
                    authToken, amountCents, request.MerchantReference, cancellationToken);

                var paymentKeyToken = await GetClassicPaymentKeyAsync(
                    authToken, orderId, amountCents, request, cancellationToken);

                var payHttpRequest = new HttpRequestMessage(
                    HttpMethod.Post, $"{_settings.BaseUrl}/api/acceptance/payments/pay")
                {
                    Content = JsonContent.Create(new
                    {
                        source = new
                        {
                            identifier = request.SavedCardToken,
                            subtype = "TOKEN"
                        },
                        payment_token = paymentKeyToken
                    })
                };

                var payResponse = await _httpClient.SendAsync(
                    payHttpRequest, cancellationToken);

                var payRawBody = await payResponse.Content
                    .ReadAsStringAsync(cancellationToken);

                Console.WriteLine($"Pay-with-token response ({(int)payResponse.StatusCode}): {payRawBody}");

                if (!payResponse.IsSuccessStatusCode)
                {
                    return new GatewayChargeResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Paymob pay-with-token failed ({(int)payResponse.StatusCode}): {payRawBody}"
                    };
                }

                using var payDoc = JsonDocument.Parse(payRawBody);
                var payRoot = payDoc.RootElement;

                // Some issuers require a 3-D Secure challenge even for a
                // saved token - if Paymob wants us to redirect, it comes
                // back as one of these fields depending on integration
                // type. Check the raw response if this ever comes back
                // null but a 3DS prompt was expected on the Paymob
                // dashboard's transaction log.
                var redirectUrl =
                    GetStringOrNull(payRoot, "redirect_url") ??
                    GetStringOrNull(payRoot, "iframe_redirection_url") ??
                    GetStringOrNull(payRoot, "redirection_url");

                var pending =
                    payRoot.TryGetProperty("pending", out var pendingEl) &&
                    pendingEl.ValueKind == JsonValueKind.True;

                var success =
                    payRoot.TryGetProperty("success", out var successEl) &&
                    successEl.ValueKind == JsonValueKind.True;

                // "Initiation failed" (bad token, card declined
                // outright, etc.) is different from "pending, waiting on
                // 3DS or the async webhook" - only the former is a hard
                // failure here.
                if (!success && !pending && redirectUrl is null)
                {
                    var declineReason = GetStringOrNull(payRoot, "data")
                        ?? "The saved card declined this charge.";

                    return new GatewayChargeResult
                    {
                        IsSuccess = false,
                        ErrorMessage = declineReason
                    };
                }

                return new GatewayChargeResult
                {
                    IsSuccess = true,
                    GatewayTransactionId = request.MerchantReference,
                    CheckoutUrl = redirectUrl
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

        private async Task<string> GetClassicAuthTokenAsync(
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/api/auth/tokens",
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

        private async Task<long> CreateClassicOrderAsync(
            string authToken,
            int amountCents,
            string merchantReference,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/api/ecommerce/orders",
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

        private async Task<string> GetClassicPaymentKeyAsync(
            string authToken,
            long orderId,
            int amountCents,
            GatewayChargeRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/api/acceptance/payment_keys",
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

        private static string? GetStringOrNull(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var el))
            {
                return null;
            }

            return el.ValueKind switch
            {
                JsonValueKind.String => el.GetString(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => el.GetRawText()
            };
        }

        public async Task<GatewayRefundResult> RefundAsync(
            GatewayRefundRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var amountCents = (int)(request.Amount * 100);

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
                Console.WriteLine("No signature received");
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                if (!root.TryGetProperty("obj", out var obj))
                {
                    Console.WriteLine("Missing 'obj' in payload");
                    return false;
                }

                var type = root.TryGetProperty("type", out var typeEl)
                    ? typeEl.GetString()
                    : null;

                Console.WriteLine($"Type: {type}");

                if (type == "TRANSACTION")
                {
                    var concatenated = BuildTransactionHmacString(obj);
                    return VerifyHmac(concatenated, receivedSignature);
                }
                else if (type == "TOKEN")
                {
                    var concatenated = BuildTokenHmacString(obj);
                    return VerifyHmac(concatenated, receivedSignature);
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        private bool VerifyHmac(string concatenated, string receivedSignature)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
            var messageBytes = Encoding.UTF8.GetBytes(concatenated);

            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            var expectedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            Console.WriteLine($"Concatenated: {concatenated}");
            Console.WriteLine($"Expected: {expectedSignature}");
            Console.WriteLine($"Received: {receivedSignature.ToLowerInvariant()}");

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(receivedSignature.ToLowerInvariant()));
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

            var sourcePan = GetNestedRawOrNull(obj, "source_data", "pan");
            var sourceSubType = GetNestedRawOrNull(obj, "source_data", "sub_type");
            var sourceType = GetNestedRawOrNull(obj, "source_data", "type");

            return new GatewayWebhookEvent
            {
                EventType = GatewayWebhookEventType.Transaction,
                GatewayTransactionId = merchantReference,
                ProviderOrderId = GetNestedRawOrNull(obj, "order", "id"),
                ProviderTransactionId = GetRawOrNull(obj, "id"),
                IsSuccessful = isSuccessful,
                Amount = amountCents / 100m,

                MaskedPan = sourcePan != null ? $"•••• {sourcePan}" : null,
                CardSubType = sourceSubType,
                CardToken = null
            };
        }

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
                GetRaw(obj, "token")
            );
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

        private class AuthTokenResponse
        {
            [JsonPropertyName("token")]
            public string? Token { get; set; }
        }

        private class OrderResponse
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
        }

        private class PaymentKeyResponse
        {
            [JsonPropertyName("token")]
            public string? Token { get; set; }
        }
    }
}
