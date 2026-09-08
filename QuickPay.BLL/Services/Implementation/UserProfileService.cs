using QuickPay.BLL.DTOs.Profile;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.BLL.Services.Implementation
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProfileDto?> GetProfileAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                return null;
            }

            return new ProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                IsPhoneVerified = user.IsPhoneVerified,
                IsEmailVerified = user.IsEmailVerified,
                IsExternalUser = user.IsExternalUser
            };
        }

        public async Task<ProfileResultDto> UpdateProfilePhotoAsync(
            int userId,
            string photoUrl,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                return new ProfileResultDto
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            user.ProfilePhotoUrl = photoUrl;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProfileResultDto
            {
                IsSuccess = true,
                Message = "Profile photo updated successfully.",
                Profile = new ProfileDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    IsPhoneVerified = user.IsPhoneVerified,
                    IsEmailVerified = user.IsEmailVerified,
                    IsExternalUser = user.IsExternalUser
                }
            };
        }

        public async Task<ProfileResultDto> UpdateProfileAsync(
            UpdateProfileRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(
                request.CurrentUserId, cancellationToken);

            if (user is null)
            {
                return new ProfileResultDto
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            var newUserName = request.UserName?.Trim() ?? string.Empty;
            var newEmail = request.Email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(newUserName) ||
                string.IsNullOrWhiteSpace(newEmail))
            {
                return new ProfileResultDto
                {
                    IsSuccess = false,
                    Message = "Username and email are required."
                };
            }

            if (!string.Equals(newUserName, user.UserName, StringComparison.OrdinalIgnoreCase) &&
                await _unitOfWork.Users.ExistsByUserNameAsync(newUserName, cancellationToken))
            {
                return new ProfileResultDto
                {
                    IsSuccess = false,
                    Message = "This username is already taken."
                };
            }

            if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase) &&
                await _unitOfWork.Users.ExistsByEmailAsync(newEmail, cancellationToken))
            {
                return new ProfileResultDto
                {
                    IsSuccess = false,
                    Message = "This email is already in use."
                };
            }

            var emailChanged = !string.Equals(
                newEmail, user.Email, StringComparison.OrdinalIgnoreCase);

            user.UserName = newUserName;
            user.Email = newEmail;

            if (emailChanged)
            {
                // Changing the email invalidates the previous verification -
                // re-verification (e.g. via OTP/email link) is a follow-up
                // task, not part of this fix.
                user.IsEmailVerified = false;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProfileResultDto
            {
                IsSuccess = true,
                Message = "Profile updated successfully.",
                Profile = new ProfileDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    IsPhoneVerified = user.IsPhoneVerified,
                    IsEmailVerified = user.IsEmailVerified,
                    IsExternalUser = user.IsExternalUser
                }
            };
        }
    }
}
