using QuickPay.BLL.DTOs.Profile;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<ProfileDto?> GetProfileAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<ProfileResultDto> UpdateProfileAsync(
            UpdateProfileRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ProfileResultDto> UpdateProfilePhotoAsync(
            int userId,
            string photoUrl,
            CancellationToken cancellationToken = default);
    }
}
