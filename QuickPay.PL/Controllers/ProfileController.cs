using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.Profile;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.ViewModels.Profile;

namespace QuickPay.PL.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxPhotoSizeBytes = 2 * 1024 * 1024; // 2 MB
        private const string UploadsRelativeFolder = "uploads/profile-photos";

        private readonly IUserProfileService _profileService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(
            IUserProfileService profileService,
            ICurrentUserService currentUserService,
            IWebHostEnvironment webHostEnvironment)
        {
            _profileService = profileService;
            _currentUserService = currentUserService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var profile = await _profileService.GetProfileAsync(userId, cancellationToken);

            if (profile is null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                UserName = profile.UserName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                IsPhoneVerified = profile.IsPhoneVerified,
                IsEmailVerified = profile.IsEmailVerified,
                IsExternalUser = profile.IsExternalUser,
                ProfilePhotoUrl = profile.ProfilePhotoUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            ProfileViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _currentUserService.GetCurrentUserId();

            var result = await _profileService.UpdateProfileAsync(
                new UpdateProfileRequestDto
                {
                    CurrentUserId = userId,
                    UserName = model.UserName,
                    Email = model.Email
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(
            IFormFile? photo,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            if (photo is null || photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Please choose an image to upload.";
                return RedirectToAction(nameof(Index));
            }

            if (photo.Length > MaxPhotoSizeBytes)
            {
                TempData["ErrorMessage"] = "Image must be 2 MB or smaller.";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Only .jpg, .jpeg, .png, or .webp images are allowed.";
                return RedirectToAction(nameof(Index));
            }

            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath, UploadsRelativeFolder);
            Directory.CreateDirectory(uploadsFolder);

            // A GUID file name avoids collisions and path-traversal issues
            // from the original, user-supplied file name.
            var fileName = $"{userId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream, cancellationToken);
            }

            var photoUrl = $"/{UploadsRelativeFolder}/{fileName}";

            var result = await _profileService.UpdateProfilePhotoAsync(
                userId, photoUrl, cancellationToken);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Profile photo updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
