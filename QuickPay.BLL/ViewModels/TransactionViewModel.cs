using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; }

        public string FromAccountName { get; set; } = null!;

        public string ToAccountName { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Type { get; set; } = null!;

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
