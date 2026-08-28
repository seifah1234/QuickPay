using System.ComponentModel.DataAnnotations;

namespace QuickPay.BLL.ViewModels.Auth
{
    public class AddPhoneNumberViewModel
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
