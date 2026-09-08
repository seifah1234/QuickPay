using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    public class TransactionHistoryController : Controller
    {
        private readonly ITransactionHistoryService _transactionHistoryService;

        public TransactionHistoryController(ITransactionHistoryService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var transactions = await _transactionHistoryService.GetUserHistoryAsync(
                userId, pageNumber, pageSize);

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.HasNext = transactions.Count() == pageSize;

            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string filterBy = "", string filterValue = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var transactions = await _transactionHistoryService.GetUserHistoryAsync(
                userId, pageNumber, pageSize, filterBy, filterValue);

            var result = transactions.Select(t => new
            {
                id = t.Id,
                type = t.Type,
                amount = t.Amount,
                status = t.Status,
                createdAt = t.CreatedAt,
                direction = t.Direction
            });

            return Json(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}