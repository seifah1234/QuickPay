using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Abstraction;

namespace QuickPay.PL.Controllers
{
  
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

       
        public async Task<IActionResult> Index(int userId = 1)
        {
            ViewBag.UserId = userId;
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return View(notifications);
        }

      

      
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id, int userId)
        {
            await _notificationService.MarkAsReadAsync(id);
            return RedirectToAction("Index", new { userId });
        }
    }
}
