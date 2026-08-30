using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationsController(
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var notifications = await _notificationService
                .GetUserNotificationsAsync(userId, cancellationToken);

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(
            int id,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            await _notificationService.MarkAsReadAsync(
                id, userId, cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}
