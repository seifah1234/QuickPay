using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.DTOs.Admin;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;

        public AdminController(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAuditLogService auditLogService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
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
            ViewBag.CurrentUserId = userId;
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
                Type = "Personal",
                IsShared = false,
                Balance = w.Balance,
                CreatedAt = w.CreatedAt,
                Currency = w.Currency,
                IsActive = w.IsActive,
                OwnerDisplay = w.User != null ? $"{w.User.UserName} ({w.User.Email})" : "N/A"
            }).Concat(sharedWallets.Select(sw => new AdminWalletDto
            {
                Id = sw.Id,
                Name = sw.Name,
                Type = "Shared",
                IsShared = true,
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
            var adminId = _currentUserService.GetCurrentUserId();

            if (id == adminId)
            {
                TempData["ErrorMessage"] = "You cannot block your own account.";
                return RedirectToAction("Users");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                adminId,
                user.IsActive ? "ActivateUser" : "BlockUser",
                "User",
                user.Id,
                $"{user.UserName} was {(user.IsActive ? "activated" : "blocked")} by an admin.",
                cancellationToken);

            await _notificationService.NotifyAsync(
                user.Id,
                user.IsActive ? "AccountActivated" : "AccountBlocked",
                user.IsActive
                    ? "Your account has been reactivated by an admin."
                    : "Your account has been blocked by an admin. Contact support for help.",
                cancellationToken);

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdminRole(int id, CancellationToken cancellationToken)
        {
            var adminId = _currentUserService.GetCurrentUserId();

            if (id == adminId)
            {
                TempData["ErrorMessage"] = "You cannot change your own admin role.";
                return RedirectToAction("Users");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            user.IsAdmin = !user.IsAdmin;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                adminId,
                user.IsAdmin ? "PromoteToAdmin" : "DemoteFromAdmin",
                "User",
                user.Id,
                $"{user.UserName} was {(user.IsAdmin ? "promoted to" : "demoted from")} Admin.",
                cancellationToken);

            await _notificationService.NotifyAsync(
                user.Id,
                user.IsAdmin ? "PromotedToAdmin" : "DemotedFromAdmin",
                user.IsAdmin
                    ? "You have been granted Admin access."
                    : "Your Admin access has been revoked.",
                cancellationToken);

            TempData["SuccessMessage"] =
                $"{user.UserName} is {(user.IsAdmin ? "now an Admin" : "no longer an Admin")}. " +
                "They will need to log in again for the change to take effect.";

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleWalletActive(
            int id,
            bool isShared,
            CancellationToken cancellationToken)
        {
            var adminId = _currentUserService.GetCurrentUserId();

            if (isShared)
            {
                var sharedWallet = await _unitOfWork.SharedWallets.GetWithMembersAsync(
                    id, cancellationToken);

                if (sharedWallet is null)
                {
                    return NotFound();
                }

                sharedWallet.IsActive = !sharedWallet.IsActive;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _auditLogService.LogAsync(
                    adminId,
                    sharedWallet.IsActive ? "ActivateWallet" : "BlockWallet",
                    "SharedWallet",
                    sharedWallet.Id,
                    $"Shared wallet \"{sharedWallet.Name}\" was {(sharedWallet.IsActive ? "activated" : "blocked")} by an admin.",
                    cancellationToken);

                var message = sharedWallet.IsActive
                    ? $"Your shared wallet \"{sharedWallet.Name}\" has been reactivated by an admin."
                    : $"Your shared wallet \"{sharedWallet.Name}\" has been blocked by an admin.";

                foreach (var member in sharedWallet.Members)
                {
                    await _notificationService.NotifyAsync(
                        member.UserId,
                        sharedWallet.IsActive ? "WalletActivated" : "WalletBlocked",
                        message,
                        cancellationToken);
                }
            }
            else
            {
                var wallet = await _unitOfWork.Wallets.GetByIdAsync(id, cancellationToken);

                if (wallet is null)
                {
                    return NotFound();
                }

                wallet.IsActive = !wallet.IsActive;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _auditLogService.LogAsync(
                    adminId,
                    wallet.IsActive ? "ActivateWallet" : "BlockWallet",
                    "Wallet",
                    wallet.Id,
                    $"Wallet \"{wallet.Name}\" was {(wallet.IsActive ? "activated" : "blocked")} by an admin.",
                    cancellationToken);

                await _notificationService.NotifyAsync(
                    wallet.UserId,
                    wallet.IsActive ? "WalletActivated" : "WalletBlocked",
                    wallet.IsActive
                        ? $"Your wallet \"{wallet.Name}\" has been reactivated by an admin."
                        : $"Your wallet \"{wallet.Name}\" has been blocked by an admin.",
                    cancellationToken);
            }

            return RedirectToAction("Wallets");
        }
    }
}
