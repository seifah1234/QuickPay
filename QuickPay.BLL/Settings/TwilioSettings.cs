namespace QuickPay.BLL.Settings
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;

        public string AuthToken { get; set; } = string.Empty;

        public string FromPhoneNumber { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(AccountSid) &&
            !AccountSid.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(AuthToken) &&
            !AuthToken.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(FromPhoneNumber) &&
            !FromPhoneNumber.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase);
    }
}
