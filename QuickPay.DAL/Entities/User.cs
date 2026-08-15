namespace QuickPay.DAL.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public ICollection<Wallet> Wallets { get; set; }
            = new List<Wallet>();

        public ICollection<SharedWallet> OwnedSharedWallets { get; set; }
            = new List<SharedWallet>();

        public ICollection<SharedWalletMember> SharedWalletMemberships { get; set; }
            = new List<SharedWalletMember>();
    }
}