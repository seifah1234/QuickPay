namespace QuickPay.BLL.DTOs.Auth;

public class ExternalLoginRequestDto
{
    public  string Email { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    // public string Token { get; set; } = null!;
    // public DateTime TokenTtl { get; set; }
    // public DateTime LastUpdatedAt { get; set; }
}