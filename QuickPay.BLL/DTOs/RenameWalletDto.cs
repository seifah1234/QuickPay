namespace QuickPay.BLL.DTOs
{
    public class RenameWalletDto
    {
        public int CurrentUserId { get; set; }

        public int WalletId { get; set; }

        public string NewName { get; set; } = string.Empty;
    }
}
