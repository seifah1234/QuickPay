using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.DTOs
{
    public class TransferResultDto
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        public int FromAccountId { get; set; }

        public int ToAccountId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
