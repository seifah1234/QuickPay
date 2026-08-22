using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class SplitParticipantRepository : ISplitParticipantRepository
    {
        private readonly QuickPayDbContext _context;

        public SplitParticipantRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<SplitParticipant?> GetAsync(
            int splitGroupId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SplitParticipant>()
                .Include(x => x.SplitGroup)
                    .ThenInclude(g => g.Payment)
                .FirstOrDefaultAsync(
                    x => x.SplitGroupId == splitGroupId && x.UserId == userId,
                    cancellationToken);
        }

        public async Task AddRangeAsync(
            IEnumerable<SplitParticipant> participants,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<SplitParticipant>()
                .AddRangeAsync(participants, cancellationToken);
        }
    }
}
