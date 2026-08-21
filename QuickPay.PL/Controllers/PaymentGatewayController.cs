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

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(
            int walletId,
            decimal amount,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var result = await _gatewayService.InitiateDepositAsync(
                new InitiateDepositRequestDto
                {
                    CurrentUserId = userId,
                    WalletId = walletId,
                    Amount = amount
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Deposit));
            }

            return Redirect(result.CheckoutUrl!);
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult FakeCheckout(string id, decimal amount)
        {
            ViewBag.GatewayTransactionId = id;
            ViewBag.Amount = amount;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FakeCheckoutSubmit(
            string gatewayTransactionId,
            decimal amount,
            bool approve,
            CancellationToken cancellationToken)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                GatewayTransactionId = gatewayTransactionId,
                IsSuccessful = approve,
                Amount = amount
            });

            var signature = FakePaymentGatewaySignature(payload);

            using var content = new StringContent(
                payload, System.Text.Encoding.UTF8, "application/json");

            var isProcessed = await _gatewayService.HandleWebhookAsync(
                payload,
                new Dictionary<string, string>(),
                signature,
                cancellationToken);

            TempData[isProcessed ? "SuccessMessage" : "ErrorMessage"] =
                isProcessed
                    ? "Payment processed."
                    : "Could not process the payment.";

            return RedirectToAction(nameof(Deposit));
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

        private static string FakePaymentGatewaySignature(string rawBody)
        {
            return QuickPay.BLL.Services.Implementation
                .FakePaymentGatewayProvider.ComputeSignature(rawBody);
        }
    }
}
