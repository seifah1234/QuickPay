namespace QuickPay.BLL.DTOs.SmartSplit
{
    public class SplitParticipantInputDto
    {
        /// <summary>Username, email, or phone number of the participant.</summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>Required for SplitType.CustomAmount.</summary>
        public decimal? Amount { get; set; }

        /// <summary>Required for SplitType.Percentage.</summary>
        public decimal? Percentage { get; set; }
    }

    public class CreateSplitGroupDto
    {
        public int CurrentUserId { get; set; }

        /// <summary>Which of the initiator's own wallets collects payments as participants pay in.</summary>
        public int SettlementWalletId { get; set; }

        public decimal TotalAmount { get; set; }

        public string SplitType { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public string? Description { get; set; }

        public List<SplitParticipantInputDto> Participants { get; set; } = new();
    }

    public class SplitGroupDto
    {
        public int Id { get; set; }

        public int InitiatorUserId { get; set; }

        public string InitiatorUserName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string Currency { get; set; } = "EGP";

        public string SplitType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public IEnumerable<SplitParticipantDto> Participants { get; set; }
            = Enumerable.Empty<SplitParticipantDto>();
    }

    public class SplitParticipantDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal? Percentage { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? PaidAt { get; set; }
    }

    public class PaySplitShareDto
    {
        public int SplitGroupId { get; set; }

        public int CurrentUserId { get; set; }

        public int FromWalletId { get; set; }
    }

    public class SendSplitReminderDto
    {
        public int SplitGroupId { get; set; }

        public int CurrentUserId { get; set; }

        /// <summary>Null = remind every participant who still hasn't paid.</summary>
        public int? ParticipantUserId { get; set; }
    }

    public class CancelSplitGroupDto
    {
        public int SplitGroupId { get; set; }

        public int CurrentUserId { get; set; }
    }
}
