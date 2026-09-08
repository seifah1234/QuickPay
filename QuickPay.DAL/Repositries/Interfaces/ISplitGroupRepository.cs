using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface ISplitGroupRepository
    {
        /// <summary>Includes Payment (+ SettlementWallet + initiator User) and Participants (+ User).</summary>
        Task<SplitGroup?> GetWithDetailsAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>Splits where the user is either the initiator or a participant.</summary>
        Task<IEnumerable<SplitGroup>> GetForUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            SplitGroup splitGroup,
            CancellationToken cancellationToken = default);
    }
}
