namespace QuickPay.BLL.DTOs.Profile
{
    public class UpdateProfileRequestDto
    {
        public int CurrentUserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
