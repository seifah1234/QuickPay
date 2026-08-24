using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public class AuditLog : BaseEntity
    {
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;

      
        public string Action { get; set; } = string.Empty;

 
        public string EntityAffected { get; set; } = string.Empty;

      
        public int? AffectedEntityId { get; set; }

        
        public string? Details { get; set; }
    }
}
