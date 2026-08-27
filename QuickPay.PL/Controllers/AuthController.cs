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

        public AuthController(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
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

            return RedirectToAction("Index", "Home");
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

            return RedirectToAction("Index", "Home");
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
            // Authenticate against temporary external cookie scheme
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
            // Clean up temporary external cookie
            await HttpContext.SignOutAsync("ExternalCookie");
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Login));
            }
            // Set custom access_token and refresh_token cookies
            SetAuthCookies(result);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }


        private void SetAuthCookies(AuthResultDto result)
        {
            Response.Cookies.Append(
                AccessTokenCookie,
                result.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.AccessTokenExpiresAt
                });

            Response.Cookies.Append(
                RefreshTokenCookie,
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.RefreshTokenExpiresAt
                });
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete(AccessTokenCookie);
            Response.Cookies.Delete(RefreshTokenCookie);
        }
    }
}
