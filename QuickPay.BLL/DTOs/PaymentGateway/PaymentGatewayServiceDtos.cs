namespace QuickPay.BLL.DTOs.PaymentGateway
{
    public class InitiateDepositRequestDto
    {
        public int CurrentUserId { get; set; }

        public int WalletId { get; set; }

        public decimal Amount { get; set; }
    }

    public class InitiateWithdrawRequestDto
    {
        public int CurrentUserId { get; set; }

        public int WalletId { get; set; }

        public int BankAccountId { get; set; }

        public decimal Amount { get; set; }
    }

    public class GatewayInitiationResultDto
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? CheckoutUrl { get; set; }
    }

    public class LinkBankAccountRequestDto
    {
        public int CurrentUserId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string MaskedNumber { get; set; } = string.Empty;

        public string GatewayToken { get; set; } = string.Empty;
    }

    public class BankAccountDto
    {
        public int Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string MaskedNumber { get; set; } = string.Empty;
    }
}
