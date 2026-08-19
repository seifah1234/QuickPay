using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class WalletController : Controller
{
    private readonly IWalletService _walletService;
    private readonly ICurrentUserService _currentUserService;

    public WalletController(
        IWalletService walletService,
        ICurrentUserService currentUserService)
    {
        _walletService = walletService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var wallets = await _walletService.GetMyWalletsAsync(
            userId, cancellationToken);

        var model = new WalletIndexViewModel
        {
            Wallets = wallets.Select(w => new WalletViewModel
            {
                Id = w.Id,
                Name = w.Name,
                Balance = w.Balance,
                Currency = w.Currency
            })
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string newWalletName,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        try
        {
            await _walletService.CreateWalletAsync(
                new CreateWalletDto
                {
                    CurrentUserId = userId,
                    Name = newWalletName
                },
                cancellationToken);

            TempData["SuccessMessage"] = "Wallet created.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(
        int walletId,
        string newName,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        try
        {
            await _walletService.RenameWalletAsync(
                new RenameWalletDto
                {
                    CurrentUserId = userId,
                    WalletId = walletId,
                    NewName = newName
                },
                cancellationToken);

            TempData["SuccessMessage"] = "Wallet renamed.";
        }
        catch (Exception ex) when (
            ex is ArgumentException or
            KeyNotFoundException or
            UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int walletId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        try
        {
            await _walletService.DeleteWalletAsync(
                walletId, userId, cancellationToken);

            TempData["SuccessMessage"] = "Wallet deleted.";
        }
        catch (Exception ex) when (
            ex is InvalidOperationException or
            KeyNotFoundException or
            UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(
        int walletId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        try
        {
            await _walletService.DepositAsync(
                new DepositWithdrawRequestDto
                {
                    CurrentUserId = userId,
                    WalletId = walletId,
                    Amount = amount
                },
                cancellationToken);

            TempData["SuccessMessage"] = "Deposit successful.";
        }
        catch (Exception ex) when (
            ex is ArgumentException or
            InvalidOperationException or
            KeyNotFoundException or
            UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(
        int walletId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        try
        {
            await _walletService.WithdrawAsync(
                new DepositWithdrawRequestDto
                {
                    CurrentUserId = userId,
                    WalletId = walletId,
                    Amount = amount
                },
                cancellationToken);

            TempData["SuccessMessage"] = "Withdrawal successful.";
        }
        catch (Exception ex) when (
            ex is ArgumentException or
            InvalidOperationException or
            KeyNotFoundException or
            UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
