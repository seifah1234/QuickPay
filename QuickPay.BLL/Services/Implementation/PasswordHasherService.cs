using Microsoft.AspNetCore.Identity;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;

namespace QuickPay.BLL.Services.Implementation
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword(new User(), password);
        }

        public bool Verify(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(
                new User(),
                hashedPassword,
                providedPassword);

            return result is PasswordVerificationResult.Success
                or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
