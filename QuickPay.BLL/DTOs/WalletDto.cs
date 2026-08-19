namespace QuickPay.BLL.DTOs
{
    public class WalletDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "EGP";

        public bool IsActive { get; set; }
    }
}
