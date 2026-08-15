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
