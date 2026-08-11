using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.ViewModels;

public class TransferController : Controller
{
    private readonly ITransferService _transferService;
    private readonly IFinancialAccountService _accountService;
    private readonly IMapper _mapper;

    public TransferController(
        ITransferService transferService,
        IFinancialAccountService accountService,
        IMapper mapper)
    {
        _transferService = transferService;
        _accountService = accountService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        var accounts =
            await _accountService
                .GetAvailableAccountsAsync(cancellationToken);

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
        if (!ModelState.IsValid)
        {
            await LoadAccounts(model, cancellationToken);

            return View(model);
        }

        try
        {
            var request =
                _mapper.Map<TransferRequestDto>(model);

            var result =
                await _transferService.TransferAsync(
                    request,
                    cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message);

                await LoadAccounts(model, cancellationToken);

                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadAccounts(model, cancellationToken);

            return View(model);
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadAccounts(model, cancellationToken);

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadAccounts(model, cancellationToken);

            return View(model);
        }
    }

    private async Task LoadAccounts(
    TransferViewModel model,
    CancellationToken cancellationToken)
    {
        var accounts =
            await _accountService
                .GetAvailableAccountsAsync(cancellationToken);

        model.Accounts =
            _mapper.Map<IEnumerable<AccountViewModel>>(
                accounts);
    }

    private async Task LoadAccounts(
        CancellationToken cancellationToken)
    {
        ViewBag.Accounts =
            await _accountService
                .GetAvailableAccountsAsync(
                    cancellationToken);
    }
}
