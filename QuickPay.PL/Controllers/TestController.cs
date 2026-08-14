using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Abstraction;

namespace QuickPay.PL.Controllers
{
    public class TestController : Controller
    {
        private readonly INotificationService _notificationService;

        public TestController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Send(int userId, string type, string message)
        {
            await _notificationService.NotifyAsync(userId, type, message);
            return RedirectToAction("Index", "Notifications", new { userId });
        }
    }
}
