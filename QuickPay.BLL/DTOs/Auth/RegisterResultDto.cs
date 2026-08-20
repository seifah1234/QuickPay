namespace QuickPay.BLL.DTOs.Auth
{
    public class RegisterResultDto
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public int UserId { get; set; }
    }
}