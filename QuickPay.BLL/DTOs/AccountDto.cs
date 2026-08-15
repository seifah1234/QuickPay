using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }

        public string AccountType { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "EGP";

        public bool IsActive { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }
}
