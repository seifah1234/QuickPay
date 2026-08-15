using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Entities
{
    public class SharedWalletMember : BaseEntity
    {
        public int SharedWalletId { get; set; }

        public SharedWallet SharedWallet { get; set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public SharedWalletRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}