using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;
using System.Security.Cryptography;

namespace QuickPay.BLL.Services.Implementation
{
    public class OtpService : IOtpService
    {
        private const int CodeLength = 6;
        private const int ExpiryMinutes = 5;
        private const int MaxAttempts = 5;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IOtpDeliveryService _otpDelivery;

        public OtpService(
            IUnitOfWork unitOfWork,
            IPasswordHasherService passwordHasher,
            IOtpDeliveryService otpDelivery)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _otpDelivery = otpDelivery;
        }

        public async Task GenerateAndSendAsync(
            int userId,
            string phoneNumber,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            var code = GenerateNumericCode();

            var otpCode = new OtpCode
            {
                UserId = userId,
                CodeHash = _passwordHasher.Hash(code),
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
                IsUsed = false,
                AttemptCount = 0
            };

            await _unitOfWork.OtpCodes.AddAsync(otpCode, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _otpDelivery.SendAsync(phoneNumber, code, cancellationToken);
        }

        public async Task<bool> VerifyAsync(
            int userId,
            string code,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            var otpCode = await _unitOfWork.OtpCodes.GetLatestActiveAsync(
                userId,
                purpose,
                cancellationToken);

            if (otpCode is null)
            {
                return false;
            }

            if (otpCode.IsExpired || otpCode.AttemptCount >= MaxAttempts)
            {
                return false;
            }

            var isMatch = _passwordHasher.Verify(otpCode.CodeHash, code);

            if (!isMatch)
            {
                otpCode.AttemptCount++;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return false;
            }

            otpCode.IsUsed = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static string GenerateNumericCode()
        {
            var maxExclusive = (int)Math.Pow(10, CodeLength);
            var value = RandomNumberGenerator.GetInt32(0, maxExclusive);
            return value.ToString(new string('0', CodeLength));
        }
    }
}
