using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repositries.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AuditLog>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string? filterBy = null,
            string? filterValue = null,
            CancellationToken cancellationToken = default);
    }
}
