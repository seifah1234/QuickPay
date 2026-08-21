using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Entities
{
    public class PaymentGatewayTransaction : BaseEntity
    {
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int? WalletId { get; set; }

        public Wallet? Wallet { get; set; }

        public PaymentGatewayDirection Direction { get; set; }

        public int? BankAccountId { get; set; }

        public BankAccount? BankAccount { get; set; }

        public string GatewayTransactionId { get; set; } = null!;

        public string? ProviderTransactionId { get; set; }

        // Paymob's numeric order id, captured from the TRANSACTION callback.
        // Needed because the card-saving token arrives in a SEPARATE "TOKEN"
        // callback that only carries this order id (not our merchant
        // reference), so this is how we match it back to this row.
        public string? ProviderOrderId { get; set; }

        public decimal Amount { get; set; }

        public PaymentGatewayTransactionStatus Status { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}
