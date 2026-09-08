using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;
using QuickPay.DAL.Repositries.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Repositries.Implementaion
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly QuickPayDbContext _context;

        public AuditLogRepository(QuickPayDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default)
        {
            await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string? filterBy = null,
            string? filterValue = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterBy) &&
                !string.IsNullOrWhiteSpace(filterValue))
            {
                switch (filterBy)
                {
                    case "action":
                        query = query.Where(a =>
                            a.Action.Contains(filterValue));
                        break;

                    case "entity":
                        query = query.Where(a =>
                            a.EntityAffected == filterValue);
                        break;

                    case "user":
                        query = query.Where(a =>
                            a.User.UserName.Contains(filterValue));
                        break;

                    case "date":
                        var dates = filterValue.Split('|');

                        if (dates.Length == 2 &&
                            DateTime.TryParse(dates[0], out var fromDate) &&
                            DateTime.TryParse(dates[1], out var toDate))
                        {
                            fromDate = fromDate.Date;
                            toDate = toDate.Date.AddDays(1);

                            query = query.Where(a =>
                                a.CreatedAt >= fromDate &&
                                a.CreatedAt < toDate);
                        }
                        break;
                }
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
