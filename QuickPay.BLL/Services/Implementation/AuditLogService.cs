using QuickPay.BLL.DTOs;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL.Entities;
using QuickPay.DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Implementation
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(
            int userId,
            string action,
            string entityAffected,
            int? affectedEntityId = null,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityAffected = entityAffected,
                AffectedEntityId = affectedEntityId,
                Details = details
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string? filterBy = null,
            string? filterValue = null,
            CancellationToken cancellationToken = default)
        {
            var logs = await _unitOfWork.AuditLogs.GetAllAsync(
                pageNumber, pageSize, filterBy, filterValue, cancellationToken);

            return logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserName = l.User.UserName,
                Action = l.Action,
                EntityAffected = l.EntityAffected,
                AffectedEntityId = l.AffectedEntityId,
                Details = l.Details,
                CreatedAt = l.CreatedAt
            });
        }
    }
}
