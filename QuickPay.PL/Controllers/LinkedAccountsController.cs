using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    public class LinkedAccountsController : Controller
    {
        private readonly ILinkedAccountsService _linkedAccountsService;
        private readonly IPaymentGatewayService _gatewayService;
        private readonly ICurrentUserService _currentUserService;

        public LinkedAccountsController(
            ILinkedAccountsService linkedAccountsService,
            IPaymentGatewayService gatewayService,
            ICurrentUserService currentUserService)
        {
            _linkedAccountsService = linkedAccountsService;
            _gatewayService = gatewayService;
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
        public async Task<IActionResult> LinkStart(
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var result = await _gatewayService.InitiateLinkCardAsync(
                userId, cancellationToken);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return Redirect(result.CheckoutUrl!);
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
