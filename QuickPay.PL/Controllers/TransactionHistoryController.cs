using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Abstraction;

namespace QuickPay.PL.Controllers
{
    public class TransactionHistoryController : Controller
    {
        private readonly ITransactionHistoryService _historyService;

        public TransactionHistoryController(ITransactionHistoryService historyService)
        {
            _historyService = historyService;
        }


        public async Task<IActionResult> Index(int userId = 1, int pageNumber = 1, int pageSize = 20)
        {
            ViewBag.UserId = userId;
            var history = await _historyService.GetUserHistoryAsync(userId, pageNumber, pageSize);
            return View(history);
        }
    }
}
