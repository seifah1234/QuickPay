using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QuickPay.DAL.Entity
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [InverseProperty(nameof(User.Transactions))]
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
