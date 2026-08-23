using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}
