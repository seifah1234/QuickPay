namespace QuickPay.BLL.DTOs.Profile
{
    public class ProfileDto
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? ProfilePhotoUrl { get; set; }

        public bool IsPhoneVerified { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsExternalUser { get; set; }
    }
}
