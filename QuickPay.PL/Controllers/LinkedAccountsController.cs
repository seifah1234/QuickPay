using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    public class LinkedAccountsController : Controller
    {
        private readonly ILinkedAccountsService _linkedAccountsService;
        private readonly ICurrentUserService _currentUserService;

        public LinkedAccountsController(
            ILinkedAccountsService linkedAccountsService,
            ICurrentUserService currentUserService)
        {
            _linkedAccountsService = linkedAccountsService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var accounts = await _linkedAccountsService
                .GetMyLinkedAccountsAsync(userId, cancellationToken);

            return View(accounts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Link(
            string displayName,
            string maskedNumber,
            string gatewayToken,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                await _linkedAccountsService.LinkAsync(
                    new LinkBankAccountRequestDto
                    {
                        CurrentUserId = userId,
                        DisplayName = displayName,
                        MaskedNumber = maskedNumber,
                        GatewayToken = gatewayToken
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = "Account linked.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlink(
            int id,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            await _linkedAccountsService.UnlinkAsync(
                id, userId, cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}
