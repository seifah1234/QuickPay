using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.UnitOfWork;

namespace QuickPay.PL.Controllers
{
   
        [Authorize]
        public class AuditLogController : Controller
        {
            private readonly IAuditLogService _auditLogService;
            private readonly IUnitOfWork _unitOfWork;
            private readonly ICurrentUserService _currentUserService;

            public AuditLogController(
                IAuditLogService auditLogService,
                IUnitOfWork unitOfWork,
                ICurrentUserService currentUserService)
            {
                _auditLogService = auditLogService;
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

                var logs = await _auditLogService.GetAllAsync(
                    pageNumber: 1,
                    pageSize: 50,
                    cancellationToken: cancellationToken);

                return View(logs);
            }

          
            [HttpGet]
            public async Task<IActionResult> Filter(
                string? filterBy,
                string? filterValue,
                CancellationToken cancellationToken)
            {
                var logs = await _auditLogService.GetAllAsync(
                    pageNumber: 1,
                    pageSize: 50,
                    filterBy,
                    filterValue,
                    cancellationToken);

                return Json(logs);
            }
        }
    }

