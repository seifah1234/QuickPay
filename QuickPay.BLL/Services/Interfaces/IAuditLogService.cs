using QuickPay.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            int userId,
            string action,
            string entityAffected,
            int? affectedEntityId = null,
            string? details = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AuditLogDto>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string? filterBy = null,
            string? filterValue = null,
            CancellationToken cancellationToken = default);
    }

}
