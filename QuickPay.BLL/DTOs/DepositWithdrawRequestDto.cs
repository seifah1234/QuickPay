namespace QuickPay.BLL.DTOs
{
    public class DepositWithdrawRequestDto
    {
        public int CurrentUserId { get; set; }

        public int WalletId { get; set; }

        public decimal Amount { get; set; }
    }
}
