using QuickPay.DAL.Enums;

namespace QuickPay.DAL.Entities
{
    public class SplitGroup : BaseEntity
    {

        public int PaymentId { get; set; }

        public Payment Payment { get; set; } = null!;

        public SplitGroupStatus Status { get; set; }

        public DateTime? DueDate { get; set; }

        public SplitType SplitType { get; set; }


        public ICollection<SplitParticipant> Participants { get; set; }
            = new List<SplitParticipant>();
    }
}