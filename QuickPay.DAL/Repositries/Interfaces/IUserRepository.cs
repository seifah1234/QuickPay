using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        Task<User?> GetByIdentifierAsync(
            string identifier,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsByPhoneNumberAsync(
            string phoneNumber,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            User user,
            CancellationToken cancellationToken = default);
        
        Task<User?> GetByExternalLoginAsync(
            string provider,
            string providerKey,
            CancellationToken cancellationToken = default);
        Task<bool> ExistsByExternalLoginAsync(
            string provider,
            string providerKey,
            CancellationToken cancellationToken = default);
        Task CreateUserWithExternalLoginAsync(
            User user,
            string provider,
            string providerKey,
            string userName,
            CancellationToken cancellationToken = default);

        Task AddExternalLoginAsync(
            int userId,
            string provider,
            string providerKey,
            CancellationToken cancellationToken = default);
        

        Task<List<User>?> GetAllAsync(CancellationToken cancellationToken);
    }
}
