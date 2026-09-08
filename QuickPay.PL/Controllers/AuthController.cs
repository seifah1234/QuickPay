using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.Auth;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.ViewModels.Auth;
using QuickPay.DAL.Enums;

namespace QuickPay.PL.Controllers
{
    public class AuthController : Controller
    {
        private const string AccessTokenCookie = "access_token";
        private const string RefreshTokenCookie = "refresh_token";

        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(
            IAuthService authService,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = _mapper.Map<RegisterRequestDto>(model);

            var result = await _authService.RegisterAsync(
                request, cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["InfoMessage"] = result.Message;

            return RedirectToAction(
                nameof(VerifyOtp),
                new { userId = result.UserId });
        }

        [HttpGet]
        public IActionResult VerifyOtp(int userId)
        {
            return View(new VerifyOtpViewModel { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(
            VerifyOtpViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = _mapper.Map<VerifyOtpRequestDto>(model);
            request.Purpose = OtpPurpose.PhoneVerification;

            var result = await _authService.VerifyPhoneAsync(
                request, cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            SetAuthCookies(result);

            TempData["SuccessMessage"] =
                "Phone verified. You're logged in.";

            return RedirectToPostLoginDestination(result);
        }

        [HttpGet]
        public IActionResult AddPhoneNumber()
        {
            return View(new AddPhoneNumberViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(
            AddPhoneNumberViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _currentUserService.GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to add a phone number.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _authService.AddPhoneNumberAsync(
                userId, model.PhoneNumber, cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["InfoMessage"] = result.Message;

            return RedirectToAction(
                nameof(VerifyOtp),
                new { userId = userId });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = _mapper.Map<ForgotPasswordRequestDto>(model);

            var result = await _authService.ForgotPasswordAsync(request, cancellationToken);

            TempData["InfoMessage"] = result.Message;

            return RedirectToAction(nameof(ResetPassword), new { email = model.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email)
        {
            return View(new ResetPasswordViewModel { Email = email ?? string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = _mapper.Map<ResetPasswordRequestDto>(model);

            var result = await _authService.ResetPasswordAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = _mapper.Map<LoginRequestDto>(model);

            var result = await _authService.LoginAsync(
                request, cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            SetAuthCookies(result);

            return RedirectToPostLoginDestination(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refresh(
            CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookie];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }

            var result = await _authService.RefreshAsync(
                new RefreshTokenRequestDto { RefreshToken = refreshToken },
                cancellationToken);

            if (!result.IsSuccess)
            {
                ClearAuthCookies();
                return Unauthorized(result.Message);
            }

            SetAuthCookies(result);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(
            CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookie];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(
                    refreshToken, cancellationToken);
            }
            TempData["SuccessMessage"] = "Successfully logged out!";

            ClearAuthCookies();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(
    string? returnUrl = null,
    CancellationToken cancellationToken = default)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
            {
                TempData["ErrorMessage"] = "External authentication failed.";
                return RedirectToAction(nameof(Login));
            }

            var principal = authenticateResult.Principal;
            var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var fullName = principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(providerKey) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Error reading information from Google.";
                return RedirectToAction(nameof(Login));
            }

            var request = new ExternalLoginRequestDto
            {
                Provider = GoogleDefaults.AuthenticationScheme,
                ProviderKey = providerKey,
                Email = email,
                Name = fullName
            };

            var result = await _authService.ExternalLoginAsync(request, cancellationToken);

            await HttpContext.SignOutAsync("ExternalCookie");

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Login));
            }

            SetAuthCookies(result);

            TempData["SuccessMessage"] = "Successfully logged in with Google!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToPostLoginDestination(result);
        }


        private IActionResult RedirectToPostLoginDestination(AuthResultDto result)
        {
            return result.IsAdmin
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        private void SetAuthCookies(AuthResultDto result)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = result.AccessTokenExpiresAt
            };

            Response.Cookies.Append(AccessTokenCookie, result.AccessToken, cookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = result.RefreshTokenExpiresAt
            };

            Response.Cookies.Append(RefreshTokenCookie, result.RefreshToken, refreshCookieOptions);
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete(AccessTokenCookie);
            Response.Cookies.Delete(RefreshTokenCookie);
        }
    }
}
