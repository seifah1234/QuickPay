using QuickPay.DAL.Enums;

namespace QuickPay.BLL.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public int UserId { get; set; }

        public string Code { get; set; } = string.Empty;

        public OtpPurpose Purpose { get; set; }
    }
}