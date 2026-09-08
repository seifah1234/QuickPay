using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.ViewModels;
using System.Security.Claims;

namespace QuickPay.PL.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IFinancialAccountService _accountService;
        private readonly ITransactionHistoryService _transactionService;

        public HomeController(
            IFinancialAccountService accountService,
            ITransactionHistoryService transactionService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
        }


        public async Task<IActionResult> Index(
            string? payment_id = null,
            string? status = null,
            string? transaction_id = null,
            string? order_id = null,
            string? merchant_order_id = null,
            string? error_code = null,
            string? error_message = null, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var accounts = await _accountService.GetMyAccountsAsync(userId, cancellationToken);
            var recentTransactions = await _transactionService.GetUserHistoryAsync(
     userId, pageNumber: 1, pageSize: 5, cancellationToken: cancellationToken);

            var dashboardViewModel = new DashboardViewModel
            {
                Accounts = accounts,
                RecentTransactions = recentTransactions,
                TotalBalance = accounts.Where(a => a.IsActive).Sum(a => a.Balance),
                ActiveAccountsCount = accounts.Count(a => a.IsActive),
                TotalAccountsCount = accounts.Count()
            };

            return View(dashboardViewModel);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.Parse(userIdClaim ?? "0");
        }
    }
}