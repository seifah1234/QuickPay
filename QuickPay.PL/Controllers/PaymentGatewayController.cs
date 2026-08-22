using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.PaymentGateway;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    public class PaymentGatewayController : Controller
    {
        private readonly IPaymentGatewayService _gatewayService;
        private readonly ILinkedAccountsService _linkedAccountsService;
        private readonly IFinancialAccountService _financialAccountService;
        private readonly ICurrentUserService _currentUserService;

        public PaymentGatewayController(
            IPaymentGatewayService gatewayService,
            ILinkedAccountsService linkedAccountsService,
            IFinancialAccountService financialAccountService,
            ICurrentUserService currentUserService)
        {
            _gatewayService = gatewayService;
            _linkedAccountsService = linkedAccountsService;
            _financialAccountService = financialAccountService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Deposit(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            ViewBag.Accounts = await _financialAccountService
                .GetMyAccountsAsync(userId, cancellationToken);
            ViewBag.LinkedAccounts = await _linkedAccountsService
                .GetMyLinkedAccountsAsync(userId, cancellationToken);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(
            int walletId,
            decimal amount,
            int bankAccountId,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            if (bankAccountId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a card to deposit with.";
                return RedirectToAction(nameof(Deposit));
            }

            var result = await _gatewayService.InitiateDepositAsync(
                new InitiateDepositRequestDto
                {
                    CurrentUserId = userId,
                    WalletId = walletId,
                    Amount = amount,
                    BankAccountId = bankAccountId
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Deposit));
            }

            // A saved-card charge with no 3-D Secure step-up has no
            // CheckoutUrl at all - it's already been sent to Paymob and
            // is now waiting on the async webhook, same as any other
            // pending gateway transaction. Only redirect when there's
            // somewhere to redirect to.
            if (result.CheckoutUrl is not null)
            {
                return Redirect(result.CheckoutUrl);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Deposit));
        }

        [HttpGet]
        public async Task<IActionResult> Withdraw(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            ViewBag.Accounts = await _financialAccountService
                .GetMyAccountsAsync(userId, cancellationToken);
            ViewBag.LinkedAccounts = await _linkedAccountsService
                .GetMyLinkedAccountsAsync(userId, cancellationToken);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(
            int walletId,
            int bankAccountId,
            decimal amount,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var result = await _gatewayService.InitiateWithdrawAsync(
                new InitiateWithdrawRequestDto
                {
                    CurrentUserId = userId,
                    WalletId = walletId,
                    BankAccountId = bankAccountId,
                    Amount = amount
                },
                cancellationToken);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
                result.Message;

            return RedirectToAction(nameof(Withdraw));
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/payment-gateway/webhook")]
        public async Task<IActionResult> Webhook(
    CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync(cancellationToken);

            var query = Request.Query.ToDictionary(
                q => q.Key, q => q.Value.ToString());

            var signature = query.ContainsKey("hmac")
                ? query["hmac"]
                : Request.Headers["X-HMAC-SHA512"].FirstOrDefault() ?? string.Empty;

            Console.WriteLine($"Webhook received at {DateTime.UtcNow}");
            Console.WriteLine($"Query params: {string.Join(", ", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            Console.WriteLine($"Signature: {signature}");

            var processed = await _gatewayService.HandleWebhookAsync(
                rawBody, query, signature, cancellationToken);

            if (!processed)
            {
                Console.WriteLine("Webhook processing FAILED - returning BadRequest for retry");
                return BadRequest(new { received = true, processed = false });
            }

            Console.WriteLine("Webhook processed OK");
            return Ok(new { received = true, processed = true });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/payment-gateway/webhook/transaction")]
        public async Task<IActionResult> TransactionWebhook(
    CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            var query = Request.Query.ToDictionary(
                kvp => kvp.Key, kvp => kvp.Value.ToString());

            var signature = query.ContainsKey("hmac")
                ? query["hmac"]
                : Request.Headers["X-HMAC-SHA512"].FirstOrDefault() ?? string.Empty;

            Console.WriteLine($"Query params: {string.Join(", ", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            Console.WriteLine($"Signature from query/header: {signature}");

            var result = await _gatewayService.HandleWebhookAsync(
                rawBody, query, signature, cancellationToken);

            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/payment-gateway/webhook/token")]
        public async Task<IActionResult> TokenWebhook(
            CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            var query = Request.Query.ToDictionary(
                kvp => kvp.Key, kvp => kvp.Value.ToString());

            var signature = Request.Headers["X-HMAC-SHA512"].FirstOrDefault();

            var result = await _gatewayService.HandleWebhookAsync(
                rawBody, query, signature, cancellationToken);

            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

    }
}
