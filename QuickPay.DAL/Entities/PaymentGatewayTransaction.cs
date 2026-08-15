using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Entities
{
    public class PaymentGatewayTransaction : BaseEntity
    {

        public string GatewayTransactionId { get; set; } = null!;

        public decimal Amount { get; set; }

        public PaymentGatewayTransactionStatus Status { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}