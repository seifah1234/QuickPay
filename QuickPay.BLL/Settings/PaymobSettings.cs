namespace QuickPay.BLL.Settings
{
    public class PaymobSettings
    {
        public string SecretKey { get; set; } = string.Empty;

        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// The classic "API Key" from the same dashboard Settings page
        /// as SecretKey/PublicKey. Only needed for the legacy
        /// Auth→Order→Payment Key flow, used here specifically to
        /// charge a saved card token directly (Intention API has no
        /// documented direct-charge-with-token endpoint as of this
        /// writing - see PaymobGatewayProvider.ChargeWithSavedTokenAsync).
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        public int IntegrationId { get; set; }
        public int IframeId { get; set; }

        public string HmacSecret { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = "https://accept.paymob.com";


        public string NotificationUrl { get; set; } = string.Empty;
    }
}
