using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public abstract class FinancialAccount : BaseEntity
    {

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "EGP";

        public bool IsActive { get; set; } = true;

        public ICollection<Transaction> OutgoingTransactions { get; set; }
            = new List<Transaction>();

        public ICollection<Transaction> IncomingTransactions { get; set; }
            = new List<Transaction>();
    }
}
