namespace QuickPay.BLL.DTOs.Profile
{
    public class ProfileResultDto
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public ProfileDto? Profile { get; set; }
    }
}
