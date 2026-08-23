namespace QuickPay.BLL.DTOs
{
    public class CreateWalletDto
    {
        public int CurrentUserId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
