using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.SmartSplit;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Controllers
{
    [Authorize]
    public class SmartSplitController : Controller
    {
        private readonly ISmartSplitService _smartSplitService;
        private readonly IWalletService _walletService;
        private readonly ICurrentUserService _currentUserService;

        public SmartSplitController(
            ISmartSplitService smartSplitService,
            IWalletService walletService,
            ICurrentUserService currentUserService)
        {
            _smartSplitService = smartSplitService;
            _walletService = walletService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var splits = await _smartSplitService.GetMySplitsAsync(
                userId, cancellationToken);

            return View(splits);
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            int id, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                var split = await _smartSplitService.GetDetailsAsync(
                    id, userId, cancellationToken);

                ViewBag.CurrentUserId = userId;
                ViewBag.MyWallets = await _walletService.GetMyWalletsAsync(
                    userId, cancellationToken);

                return View(split);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            ViewBag.MyWallets = await _walletService.GetMyWalletsAsync(
                userId, cancellationToken);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int settlementWalletId,
            decimal totalAmount,
            string splitType,
            DateTime? dueDate,
            string? description,
            // One line per participant, format depends on splitType:
            //   Equal:        identifier
            //   CustomAmount: identifier,amount
            //   Percentage:   identifier,percentage
            string participantLines,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var participants = ParseParticipantLines(participantLines, splitType);

            try
            {
                var split = await _smartSplitService.CreateSplitAsync(
                    new CreateSplitGroupDto
                    {
                        CurrentUserId = userId,
                        SettlementWalletId = settlementWalletId,
                        TotalAmount = totalAmount,
                        SplitType = splitType,
                        DueDate = dueDate,
                        Description = description,
                        Participants = participants
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = "Split created.";

                return RedirectToAction(nameof(Details), new { id = split.Id });
            }
            catch (Exception ex) when (
                ex is ArgumentException or
                KeyNotFoundException or
                UnauthorizedAccessException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.MyWallets = await _walletService.GetMyWalletsAsync(
                    userId, cancellationToken);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(
            int id, int fromWalletId, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                await _smartSplitService.PayShareAsync(
                    new PaySplitShareDto
                    {
                        SplitGroupId = id,
                        CurrentUserId = userId,
                        FromWalletId = fromWalletId
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = "Your share was paid.";
            }
            catch (Exception ex) when (
                ex is ArgumentException or
                KeyNotFoundException or
                UnauthorizedAccessException or
                InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remind(
            int id, int? participantUserId, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                await _smartSplitService.SendReminderAsync(
                    new SendSplitReminderDto
                    {
                        SplitGroupId = id,
                        CurrentUserId = userId,
                        ParticipantUserId = participantUserId
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = "Reminder sent.";
            }
            catch (Exception ex) when (
                ex is KeyNotFoundException or
                UnauthorizedAccessException or
                InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int id, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            try
            {
                await _smartSplitService.CancelAsync(
                    new CancelSplitGroupDto
                    {
                        SplitGroupId = id,
                        CurrentUserId = userId
                    },
                    cancellationToken);

                TempData["SuccessMessage"] = "Split cancelled.";
            }
            catch (Exception ex) when (
                ex is KeyNotFoundException or
                UnauthorizedAccessException or
                InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private static List<SplitParticipantInputDto> ParseParticipantLines(
            string participantLines, string splitType)
        {
            var result = new List<SplitParticipantInputDto>();

            if (string.IsNullOrWhiteSpace(participantLines))
            {
                return result;
            }

            var lines = participantLines
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                var input = new SplitParticipantInputDto { Identifier = parts[0] };

                if (splitType == "CustomAmount" && parts.Length > 1 &&
                    decimal.TryParse(parts[1], out var amount))
                {
                    input.Amount = amount;
                }

                if (splitType == "Percentage" && parts.Length > 1 &&
                    decimal.TryParse(parts[1], out var percentage))
                {
                    input.Percentage = percentage;
                }

                result.Add(input);
            }

            return result;
        }
    }
}
