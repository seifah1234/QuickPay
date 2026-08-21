using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class UserRepository : IUserRepository
    {
        private readonly QuickPayDbContext _context;

        public UserRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Email == email,
                    cancellationToken);
        }

        public async Task<User?> GetByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    x => x.UserName == identifier ||
                         x.Email == identifier ||
                         x.PhoneNumber == identifier,
                    cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(x => x.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(x => x.UserName == userName, cancellationToken);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(
            string phoneNumber,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);
        }

        public async Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }
    }
}
