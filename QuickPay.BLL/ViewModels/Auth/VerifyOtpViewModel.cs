using System.ComponentModel.DataAnnotations;

namespace QuickPay.BLL.ViewModels.Auth
{
    public class VerifyOtpViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Enter the code we sent you.")]
        [StringLength(6, MinimumLength = 6,
            ErrorMessage = "The code is 6 digits.")]
        public string Code { get; set; } = string.Empty;

        public string? MaskedPhoneNumber { get; set; }
    }
}