using Microsoft.Extensions.Options;
using QuickPay.BLL.DTOs.Auth;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.Settings;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Enums;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IOtpService _otpService;
        private readonly JwtSettings _jwtSettings;
        private readonly int MaxUserNameAttempts = 5;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasherService passwordHasher,
            IJwtTokenService jwtTokenService,
            IOtpService otpService,
            IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _otpService = otpService;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<RegisterResultDto> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (await _unitOfWork.Users.ExistsByEmailAsync(
                    request.Email, cancellationToken))
            {
                return Fail("An account with this email already exists.");
            }

            if (await _unitOfWork.Users.ExistsByUserNameAsync(
                    request.UserName, cancellationToken))
            {
                return Fail("This username is already taken.");
            }

            if (await _unitOfWork.Users.ExistsByPhoneNumberAsync(
                    request.PhoneNumber, cancellationToken))
            {
                return Fail("An account with this phone number already exists.");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return Fail("Password is required.");
            }

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordHasher.Hash(request.Password),
                IsPhoneVerified = false,
                IsEmailVerified = false
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _otpService.GenerateAndSendAsync(
                user.Id,
                user.PhoneNumber,
                OtpPurpose.PhoneVerification,
                cancellationToken);

            return new RegisterResultDto
            {
                IsSuccess = true,
                Message = "Account created. We sent a verification code to your phone.",
                UserId = user.Id
            };

            static RegisterResultDto Fail(string message) => new()
            {
                IsSuccess = false,
                Message = message
            };
        }

        public async Task<AuthResultDto> VerifyPhoneAsync(
            VerifyOtpRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var isValid = await _otpService.VerifyAsync(
                request.UserId,
                request.Code,
                OtpPurpose.PhoneVerification,
                cancellationToken);

            if (!isValid)
            {
                return FailedAuthResult(
                    "That code is invalid or expired. Please request a new one.");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(
                request.UserId, cancellationToken);

            if (user is null)
            {
                return FailedAuthResult("Account not found.");
            }

            user.IsPhoneVerified = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResultDto> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(
                request.Email, cancellationToken);

            // Same generic message whether the email doesn't exist or the
            // password is wrong - don't reveal which one it was.
            if (user is null ||
                !_passwordHasher.Verify(user.PasswordHash, request.Password))
            {
                return FailedAuthResult("Invalid email or password.");
            }

            if (!user.IsPhoneVerified)
            {
                return FailedAuthResult(
                    "Please verify your phone number before logging in.");
            }

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResultDto> RefreshAsync(
            RefreshTokenRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existingToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(
                request.RefreshToken, cancellationToken);

            if (existingToken is null || !existingToken.IsActive)
            {
                return FailedAuthResult(
                    "Refresh token is invalid or expired. Please log in again.");
            }

            var newTokens = await IssueTokensAsync(
                existingToken.User, cancellationToken);

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.ReplacedByToken = newTokens.RefreshToken;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newTokens;
        }

        public async Task LogoutAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            var existingToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(
                refreshToken, cancellationToken);

            if (existingToken is not null && existingToken.IsActive)
            {
                existingToken.RevokedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task<AuthResultDto> IssueTokensAsync(
            User user,
            CancellationToken cancellationToken)
        {
            var (accessToken, accessTokenExpiresAt) =
                _jwtTokenService.GenerateAccessToken(user);

            var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpiryDays);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = refreshTokenExpiresAt
            };

            await _unitOfWork.RefreshTokens.AddAsync(
                refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResultDto
            {
                IsSuccess = true,
                Message = "Success.",
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        private static AuthResultDto FailedAuthResult(string message) => new()
        {
            IsSuccess = false,
            Message = message
        };

        public async Task<AuthResultDto> ExternalLoginAsync(
            ExternalLoginRequestDto? request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Provider))
                throw new ArgumentException("Provider is required.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.ProviderKey))
                throw new ArgumentException("ProviderKey is required.", nameof(request));
 
            // 1. Already linked -> just return the existing user, no writes needed.
            var linkedUser = await _unitOfWork.Users.GetByExternalLoginAsync(
                request.Provider, request.ProviderKey, cancellationToken);
 
            if (linkedUser is not null)
                return await IssueTokensAsync(linkedUser, cancellationToken);
 
            // 2. Not linked yet, but an account with this email already exists -> link it.
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var userByEmail = await _unitOfWork.Users.GetByEmailAsync(
                    request.Email, cancellationToken);
 
                if (userByEmail is not null)
                {
                    await _unitOfWork.Users.AddExternalLoginAsync(
                        userByEmail.Id, request.Provider, request.ProviderKey, cancellationToken);
 
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return await IssueTokensAsync(userByEmail, cancellationToken);
                }
            }
 
            // 3. No match at all -> brand new user, created together with the external login
            //    in a single SaveChangesAsync so both rows commit atomically.
            var userName = await GenerateUniqueUserNameAsync(request, cancellationToken);
 
            var newUser = new User
            {
                Email = request.Email,
                UserName = userName
            };
 
            await _unitOfWork.Users.CreateUserWithExternalLoginAsync(
                newUser, request.Provider, request.ProviderKey, request.Name, cancellationToken);
 
            await _unitOfWork.SaveChangesAsync(cancellationToken);
 
            return await IssueTokensAsync(newUser, cancellationToken);
        }

        private async Task<string> GenerateUniqueUserNameAsync(
                ExternalLoginRequestDto request,
                CancellationToken cancellationToken)
            {
                var baseName = BuildBaseUserName(request);
 
                var candidate = baseName;
                for (var attempt = 0; attempt < MaxUserNameAttempts; attempt++)
                {
                    var exists = await _unitOfWork.Users.ExistsByUserNameAsync(candidate, cancellationToken);
                    if (!exists)
                        return candidate;
 
                    candidate = $"{baseName}{Random.Shared.Next(1000, 9999)}";
                }
 
                // Extremely unlikely fallback: guarantees uniqueness without another DB round-trip.
                return $"{baseName}{Guid.NewGuid():N}"[..Math.Min(baseName.Length + 8, 32)];
            }
 
            private static string BuildBaseUserName(ExternalLoginRequestDto request)
            {
                var source = !string.IsNullOrWhiteSpace(request.Email)
                    ? request.Email.Split('@')[0]
                    : !string.IsNullOrWhiteSpace(request.Name)
                        ? request.Name
                        : $"{request.Provider}user";
                
                var cleaned = new string(source.Where(char.IsLetterOrDigit).ToArray());
                return string.IsNullOrEmpty(cleaned) ? "user" : cleaned.ToLowerInvariant();
            }

        public async Task<AuthResultDto> AddPhoneNumberAsync(
            int userId,
            string phoneNumber,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return FailedAuthResult("User account not found.");
            }

            if (await _unitOfWork.Users.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken))
            {
                return FailedAuthResult("An account with this phone number already exists.");
            }

            user.PhoneNumber = phoneNumber;
            user.IsPhoneVerified = false;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _otpService.GenerateAndSendAsync(
                user.Id,
                user.PhoneNumber,
                OtpPurpose.PhoneVerification,
                cancellationToken);

            return new AuthResultDto
            {
                IsSuccess = true,
                Message = "Phone number updated. We sent a verification code to your phone."
            };
        }
    }
}

