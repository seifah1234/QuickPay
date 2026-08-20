using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.DTOs
{
    public class TransferRequestDto
    {
        public int CurrentUserId { get; set; }

        public int FromAccountId { get; set; }

        public int ToAccountId { get; set; }

        public decimal Amount { get; set; }
    }
}
