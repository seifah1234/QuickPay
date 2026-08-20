using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.BLL.ViewModels
{
    public class AccountViewModel
    {
        public int Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "EGP";
    }
}
