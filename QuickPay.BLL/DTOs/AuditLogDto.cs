using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityAffected { get; set; } = string.Empty;
        public int? AffectedEntityId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
