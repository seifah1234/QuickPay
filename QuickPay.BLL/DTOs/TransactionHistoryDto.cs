namespace QuickPay.BLL.DTOs
{
    public class TransactionHistoryDto
    {
        public int Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string Direction { get; set; } = string.Empty;
    }
}
