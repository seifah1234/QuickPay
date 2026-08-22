using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    public class TransactionHistoryController : Controller
    {
        private readonly ITransactionHistoryService _historyService;
        private readonly ICurrentUserService _currentUserService;

        public TransactionHistoryController(
            ITransactionHistoryService historyService,
            ICurrentUserService currentUserService)
        {
            _historyService = historyService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var effectivePageNumber = pageNumber <= 0 ? 1 : pageNumber;
            var effectivePageSize = pageSize <= 0 ? 20 : pageSize;

            var history = await _historyService.GetUserHistoryAsync(
                userId,
                effectivePageNumber,
                effectivePageSize,
                cancellationToken);

            ViewBag.PageNumber = effectivePageNumber;

            return View(history.AsEnumerable());
        }
    }
}
