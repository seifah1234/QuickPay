namespace QuickPay.BLL.DTOs.PaymentGateway
{
    public class GatewayChargeRequest
    {
        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public string MerchantReference { get; set; } = string.Empty;

        public string PayerFullName { get; set; } = string.Empty;

        public string PayerEmail { get; set; } = string.Empty;

        public string PayerPhoneNumber { get; set; } = string.Empty;
    }

    public class GatewayChargeResult
    {
        public bool IsSuccess { get; set; }

        public string GatewayTransactionId { get; set; } = string.Empty;

        public string? CheckoutUrl { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class GatewayPayoutRequest
    {
        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public string MerchantReference { get; set; } = string.Empty;

        public string DestinationToken { get; set; } = string.Empty;
    }

    public class GatewayPayoutResult
    {
        public bool IsSuccess { get; set; }

        public string GatewayTransactionId { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }

    // Paymob sends two structurally different callbacks: a TRANSACTION
    // callback for payment success/failure, and (only when card-saving is
    // enabled) a separate TOKEN callback carrying the reusable card token.
    // They arrive as two different HTTP requests and use different HMAC
    // field orders - see PaymobGatewayProvider.
    public enum GatewayWebhookEventType
    {
        Transaction,
        CardToken
    }

    public class GatewayWebhookEvent
    {
        public GatewayWebhookEventType EventType { get; set; } =
            GatewayWebhookEventType.Transaction;

        public string GatewayTransactionId { get; set; } = string.Empty;

        public bool IsSuccessful { get; set; }

        public decimal Amount { get; set; }

        // Paymob's own transaction id (distinct from GatewayTransactionId,
        // which is our own merchant reference). Needed for refund/void calls.
        public string? ProviderTransactionId { get; set; }

        // Paymob's own numeric order id. Present on TRANSACTION callbacks
        // (obj.order.id) and on TOKEN callbacks (obj.order_id) - this is
        // what lets us match a later TOKEN callback back to the row created
        // when the TRANSACTION callback first arrived.
        public string? ProviderOrderId { get; set; }

        // Populated only on a TOKEN callback (see EventType above).
        public string? CardToken { get; set; }

        public string? MaskedPan { get; set; }

        public string? CardSubType { get; set; }
    }

    public class GatewayRefundRequest
    {
        public string GatewayTransactionId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    public class GatewayRefundResult
    {
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
