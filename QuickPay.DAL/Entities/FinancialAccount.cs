using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public abstract class FinancialAccount : BaseEntity
    {
        public string Name { get; set; } = null!;

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "EGP";

        public bool IsActive { get; set; } = true;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<Transaction> OutgoingTransactions { get; set; }
            = new List<Transaction>();

        public ICollection<Transaction> IncomingTransactions { get; set; }
            = new List<Transaction>();
    }
}
