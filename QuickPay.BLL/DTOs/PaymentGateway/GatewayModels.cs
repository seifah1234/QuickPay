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

    public class GatewayWebhookEvent
    {
        public string GatewayTransactionId { get; set; } = string.Empty;

        public bool IsSuccessful { get; set; }

        public decimal Amount { get; set; }

        // Paymob's own transaction id (distinct from GatewayTransactionId,
        // which we set to the Order id at charge time so we can correlate
        // the webhook back to the pending PaymentGatewayTransaction before
        // the real transaction id exists). Needed for refund/void calls.
        public string? ProviderTransactionId { get; set; }

        // Populated only when the integration has card-saving enabled and
        // the transaction produced a reusable token (see LinkCard flow).
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
