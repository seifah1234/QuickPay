using System.ComponentModel.DataAnnotations;

namespace QuickPay.BLL.ViewModels.Profile
{
    public class ProfileViewModel
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsPhoneVerified { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsExternalUser { get; set; }

        public string? ProfilePhotoUrl { get; set; }
    }
}
