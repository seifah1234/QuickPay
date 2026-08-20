using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public class SharedWallet : FinancialAccount
    {
        public string SharedWalletName { get; set; } = null!;

        public ICollection<SharedWalletMember> Members { get; set; }
            = new List<SharedWalletMember>();
    }
}
