using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.DTOs
{
    public class TransactionHistoryDto
    {
        public int TransactionId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
