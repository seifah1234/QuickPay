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

        public async Task<User?> GetByExternalLoginAsync(
            string provider,
            string providerKey,
            CancellationToken cancellationToken = default)
        {
            return await _context.ExternalLogins
                .Where(e => e.Provider == provider && e.ProviderUserId == providerKey)
                .Select(e => e.User)
                .FirstOrDefaultAsync(cancellationToken);
        }
 
        public async Task<bool> ExistsByExternalLoginAsync(
            string provider,
            string providerKey,
            CancellationToken cancellationToken = default)
        {
            return await _context.ExternalLogins
                .AnyAsync(e => e.Provider == provider && e.ProviderUserId == providerKey, cancellationToken);
        }
 
        public async Task CreateUserWithExternalLoginAsync(
            User user,
            string provider,
            string providerKey,
            string userName,
            CancellationToken cancellationToken = default)
        {
            user.ExternalLogins ??= new List<ExternalLogin>();
            user.ExternalLogins.Add(BuildExternalLogin(provider, providerKey));
            user.IsExternalUser = true;
            user.UserName = userName;
 
            await _context.Users.AddAsync(user, cancellationToken);
        }
 
        public async Task AddExternalLoginAsync(
            int userId,
            string provider,
            string providerKey,
            CancellationToken cancellationToken = default)
        {
            var login = BuildExternalLogin(provider, providerKey);
            login.UserId = userId;
 
            await _context.ExternalLogins.AddAsync(login, cancellationToken);
        }
 
        private static ExternalLogin BuildExternalLogin(string provider, string providerKey)
        {
            return new ExternalLogin
            {
                Provider = provider,
                ProviderUserId = providerKey
            };
        public async Task<List<User>?> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Users.ToListAsync(cancellationToken);
        }
    }
}