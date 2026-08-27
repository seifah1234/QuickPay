namespace QuickPay.DAL.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsExternalUser { get; set; } = false;

        public bool IsPhoneVerified { get; set; }

        public bool IsEmailVerified { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();

        public ICollection<OtpCode> OtpCodes { get; set; }
            = new List<OtpCode>();

        public ICollection<Wallet> Wallets { get; set; }
            = new List<Wallet>();

        public ICollection<SharedWallet> OwnedSharedWallets { get; set; }
            = new List<SharedWallet>();

        public ICollection<SharedWalletMember> SharedWalletMemberships { get; set; }
            = new List<SharedWalletMember>();
        
        public ICollection<ExternalLogin> ExternalLogins { get; set; } = new  List<ExternalLogin>();
    }
}