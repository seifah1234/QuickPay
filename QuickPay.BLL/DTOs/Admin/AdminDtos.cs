namespace QuickPay.BLL.DTOs.Admin
{
    public class AdminUserDto
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsPhoneVerified { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class AdminWalletDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string Currency { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsShared { get; set; }

        /// <summary>Owner's username for a Wallet, or a comma-separated member list for a SharedWallet.</summary>
        public string OwnerDisplay { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class AdminTransactionDto
    {
        public int Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string FromAccountName { get; set; } = string.Empty;

        public string ToAccountName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int TotalWallets { get; set; }

        public decimal TotalBalanceAcrossWallets { get; set; }

        public int TotalTransactions { get; set; }
    }
}
