namespace QuickPay.DAL.Entities;

public class ExternalLogin : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string Token { get; set; } = null!;
    public DateTime TokenTtl { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}