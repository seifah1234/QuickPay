namespace QuickPay.DAL.Entities
{
    public class BankAccount : BaseEntity
    {
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string MaskedNumber { get; set; } = null!;

        public string GatewayToken { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }
}
