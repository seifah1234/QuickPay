using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.ViewModels
{
    public class TransferResultViewModel
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        public string FromAccountName { get; set; } = string.Empty;

        public string ToAccountName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
