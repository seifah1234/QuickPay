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

        // Which of the initiator's own wallets collects the participants'
        // shares as they pay in.
        public int SettlementWalletId { get; set; }

        public Wallet SettlementWallet { get; set; } = null!;


        public SplitGroup? SplitGroup { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}