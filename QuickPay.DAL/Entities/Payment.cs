using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Entities
{
    public class Payment : BaseEntity
    {

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public PaymentStatus Status { get; set; }


        public SplitGroup? SplitGroup { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}