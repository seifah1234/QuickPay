using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly QuickPayDbContext _context;

        public PaymentRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .Include(x => x.User)
                .Include(x => x.SettlementWallet)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(
            Payment payment,
            CancellationToken cancellationToken = default)
        {
            await _context.Payments.AddAsync(payment, cancellationToken);
        }
    }
}
