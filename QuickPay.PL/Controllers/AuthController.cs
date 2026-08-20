using AutoMapper;
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
