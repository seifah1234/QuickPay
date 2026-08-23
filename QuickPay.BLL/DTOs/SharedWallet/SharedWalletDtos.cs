namespace QuickPay.BLL.DTOs.SharedWallet
{
    public class SharedWalletDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "EGP";

        public bool IsActive { get; set; }

        public IEnumerable<SharedWalletMemberDto> Members { get; set; }
            = Enumerable.Empty<SharedWalletMemberDto>();
    }

    public class SharedWalletMemberDto
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime JoinedAt { get; set; }
    }

    public class CreateSharedWalletDto
    {
        public int CurrentUserId { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class RenameSharedWalletDto
    {
        public int SharedWalletId { get; set; }

        public int CurrentUserId { get; set; }

        public string NewName { get; set; } = string.Empty;
    }

    public class AddSharedWalletMemberDto
    {
        public int SharedWalletId { get; set; }

        public int CurrentUserId { get; set; }

        /// <summary>Username, email, or phone number of the person to add.</summary>
        public string Identifier { get; set; } = string.Empty;
    }

    public class RemoveSharedWalletMemberDto
    {
        public int SharedWalletId { get; set; }

        public int CurrentUserId { get; set; }

        public int MemberUserId { get; set; }
    }

    public class CloseSharedWalletDto
    {
        public int SharedWalletId { get; set; }

        public int CurrentUserId { get; set; }
    }

    public class SharedWalletTransferDto
    {
        public int SharedWalletId { get; set; }

        public int CurrentUserId { get; set; }

        /// <summary>The member's own personal wallet, used as the other side of the transfer.</summary>
        public int PersonalWalletId { get; set; }

        public decimal Amount { get; set; }
    }
}
