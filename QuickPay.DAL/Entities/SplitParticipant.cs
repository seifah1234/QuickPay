using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Entities
{
    public class SplitParticipant : BaseEntity
    {

        public int SplitGroupId { get; set; }

        public SplitGroup SplitGroup { get; set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public decimal Amount { get; set; }

        public decimal? Percentage { get; set; }

        public SplitParticipantStatus Status { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}