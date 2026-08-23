using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.ViewModels;

public class TransferController : Controller
{
    private readonly ITransferService _transferService;
    private readonly IFinancialAccountService _accountService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public TransferController(
        ITransferService transferService,
        IFinancialAccountService accountService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _transferService = transferService;
        _accountService = accountService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var accounts =
            await _accountService
                .GetMyAccountsAsync(userId, cancellationToken);

        var model = new TransferViewModel
        {
            Accounts = _mapper.Map<IEnumerable<AccountViewModel>>(
                accounts)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
    TransferViewModel model,
    CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            await LoadAccounts(model, userId, cancellationToken);

            return View(model);
        }

        try
        {
            var request =
                _mapper.Map<TransferRequestDto>(model);

            request.CurrentUserId = userId;

            var result =
                await _transferService.TransferAsync(
                    request,
                    cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message);

                await LoadAccounts(model, userId, cancellationToken);

                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAccounts(model, userId, cancellationToken);
            return View(model);
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAccounts(model, userId, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAccounts(model, userId, cancellationToken);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAccounts(model, userId, cancellationToken);
            return View(model);
        }
    }

    /// <summary>
    /// Recipient lookup used by the "To" search box in the view
    /// (see wwwroot/js/transfer-recipient-search.js). Matches by
    /// username, phone number, or shared wallet name.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchRecipients(
        string query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var accounts =
            await _accountService.SearchRecipientAccountsAsync(
                query,
                userId,
                cancellationToken);

        var results = accounts.Select(a => new
        {
            id = a.Id,
            displayName = a.DisplayName
        });

        return Json(results);
    }

    private async Task LoadAccounts(
    TransferViewModel model,
    int userId,
    CancellationToken cancellationToken)
    {
        var accounts =
            await _accountService
                .GetMyAccountsAsync(userId, cancellationToken);

        model.Accounts =
            _mapper.Map<IEnumerable<AccountViewModel>>(
                accounts);
    }
}
