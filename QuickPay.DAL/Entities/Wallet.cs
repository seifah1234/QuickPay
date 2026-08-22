using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public class Wallet : FinancialAccount
    {
        [Column("Wallet_UserId")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
