using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.SharedWallet;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    [Authorize]
    public class SharedWalletController : Controller
    {
        private readonly ISharedWalletService _sharedWalletService;
        private readonly ICurrentUserService _currentUserService;

        public SharedWalletController(
            ISharedWalletService sharedWalletService,
            ICurrentUserService currentUserService)
        {
            _sharedWalletService = sharedWalletService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var sharedWallets = await _sharedWalletService
                .GetMySharedWalletsAsync(userId, cancellationToken);

            return View(sharedWallets);
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            int id, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                var sharedWallet = await _sharedWalletService
                    .GetDetailsAsync(id, userId, cancellationToken);

                ViewBag.History = await _sharedWalletService
                    .GetHistoryAsync(id, userId, cancellationToken);

                ViewBag.CurrentUserId = userId;

                return View(sharedWallet);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string name, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                var sharedWallet = await _sharedWalletService.CreateAsync(
                    new CreateSharedWalletDto
                    {
                        CurrentUserId = userId,
                        Name = name
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = $"\"{sharedWallet.Name}\" was created.";

                return RedirectToAction(nameof(Details), new { id = sharedWallet.Id });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(
            int id, string newName, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            await TryAsync(
                () => _sharedWalletService.RenameAsync(
                    new RenameSharedWalletDto
                    {
                        SharedWalletId = id,
                        CurrentUserId = userId,
                        NewName = newName
                    },
                    cancellationToken),
                "Renamed.");

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(
            int id, string identifier, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            await TryAsync(
                () => _sharedWalletService.AddMemberAsync(
                    new AddSharedWalletMemberDto
                    {
                        SharedWalletId = id,
                        CurrentUserId = userId,
                        Identifier = identifier
                    },
                    cancellationToken),
                "Member added.");

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(
            int id, int memberUserId, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                await _sharedWalletService.RemoveMemberAsync(
                    new RemoveSharedWalletMemberDto
                    {
                        SharedWalletId = id,
                        CurrentUserId = userId,
                        MemberUserId = memberUserId
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = "Member removed.";
            }
            catch (Exception ex) when (
                ex is KeyNotFoundException or
                UnauthorizedAccessException or
                InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(
            int id, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                await _sharedWalletService.CloseAsync(
                    new CloseSharedWalletDto
                    {
                        SharedWalletId = id,
                        CurrentUserId = userId
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = "Shared wallet closed.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (
                ex is KeyNotFoundException or
                UnauthorizedAccessException or
                InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Runs an action and turns the common domain exceptions into a
        /// TempData message instead of an unhandled 500 - every write
        /// action above just redirects back to Details either way, so the
        /// only thing that differs is the message shown.
        /// </summary>
        private async Task TryAsync(Func<Task> action, string successMessage)
        {
            try
            {
                await action();
                TempData["SuccessMessage"] = successMessage;
            }
            catch (Exception ex) when (
                ex is ArgumentException or
                KeyNotFoundException or
                UnauthorizedAccessException or
                InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
        }
    }
}
