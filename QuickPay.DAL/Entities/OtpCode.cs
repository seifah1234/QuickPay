using QuickPay.DAL.Enums;
using System;

namespace QuickPay.DAL.Entities
{
    public class OtpCode : BaseEntity
    {
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public string CodeHash { get; set; } = null!;

        public OtpPurpose Purpose { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public int AttemptCount { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
