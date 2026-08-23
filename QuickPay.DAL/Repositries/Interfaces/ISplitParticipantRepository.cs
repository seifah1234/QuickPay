using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface ISplitParticipantRepository
    {
        Task<SplitParticipant?> GetAsync(
            int splitGroupId,
            int userId,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<SplitParticipant> participants,
            CancellationToken cancellationToken = default);
    }
}
