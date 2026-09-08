using System;

namespace QuickPay.BLL.DTOs.Auth
{
    public class AuthResultDto
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// Set from the authenticated user's IsAdmin flag - lets
        /// AuthController redirect straight to the Admin dashboard on
        /// login without an extra DB round trip.
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}