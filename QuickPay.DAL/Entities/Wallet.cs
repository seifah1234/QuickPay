using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public class Wallet : FinancialAccount
    {
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
