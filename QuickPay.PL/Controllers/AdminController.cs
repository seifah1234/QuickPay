using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.Admin;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.PL.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AdminController(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }


        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var user = await _unitOfWork.Users.GetByIdAsync(
                userId, cancellationToken);

            if (user is null || !user.IsAdmin)
            {
                return StatusCode(403, "Access denied — Admins only.");
            }

            return View();
        }

        public async Task<IActionResult> Users(
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(
                userId, cancellationToken);
            if (user is null || !user.IsAdmin)
            {
                return StatusCode(403, "Access denied — Admins only.");
            }
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            return View(users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                IsPhoneVerified = u.IsPhoneVerified,
                Role = u.IsAdmin ? "Admin" : "User"
            }));
        }

        public async Task<IActionResult> Transactions(
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(
                userId, cancellationToken);
            if (user is null || !user.IsAdmin)
            {
                return StatusCode(403, "Access denied — Admins only.");
            }
            var transactions = await _unitOfWork.Transactions.GetAllAsync(cancellationToken: cancellationToken);
            return View(transactions.Select(t => new AdminTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                CreatedAt = t.CreatedAt,
                Status = t.Status.ToString(),
                Type = t.Type.ToString(),
                FromAccountName = t.FromAccount?.Name ?? "N/A",
                ToAccountName = t.ToAccount?.Name ?? "N/A"
            }));

        }

        public async Task<IActionResult> Wallets(
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(
                userId, cancellationToken);
            if (user is null || !user.IsAdmin)
            {
                return StatusCode(403, "Access denied — Admins only.");
            }
            var wallets = await _unitOfWork.Wallets.GetAllAsync(cancellationToken: cancellationToken);

            var sharedWallets = await _unitOfWork.SharedWallets.GetAllAsync(cancellationToken: cancellationToken);



            return View(wallets.Select(w => new AdminWalletDto
            {
                Id = w.Id,
                Name = w.Name,
                Balance = w.Balance,
                CreatedAt = w.CreatedAt,
                Currency = w.Currency,
                IsActive = w.IsActive,
                OwnerDisplay = w.User != null ? $"{w.User.UserName} ({w.User.Email})" : "N/A"
            }).Concat(sharedWallets.Select(sw => new AdminWalletDto
            {
                Id = sw.Id,
                Name = sw.Name,
                Balance = sw.Balance,
                CreatedAt = sw.CreatedAt,
                Currency = sw.Currency,
                IsActive = sw.IsActive,
                OwnerDisplay = sw.WalletOwner().Result != null ? $"{sw.WalletOwner().Result.UserName} ({sw.WalletOwner().Result.Email})" : "N/A"
            })));

        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserActive(int id, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;


            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return RedirectToAction("Users");

        }
    }
}
