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

        /// <summary>Used to resolve a person to invite by username, phone, or email.</summary>
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
    }
}
