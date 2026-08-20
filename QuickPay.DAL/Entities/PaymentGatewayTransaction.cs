using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Entities
{
    public class PaymentGatewayTransaction : BaseEntity
    {
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int WalletId { get; set; }

        public Wallet Wallet { get; set; } = null!;

        public PaymentGatewayDirection Direction { get; set; }

        public int? BankAccountId { get; set; }

        public BankAccount? BankAccount { get; set; }

        public string GatewayTransactionId { get; set; } = null!;

        public decimal Amount { get; set; }

        public PaymentGatewayTransactionStatus Status { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}
