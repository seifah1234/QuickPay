using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public class SharedWallet : FinancialAccount
    {
        public int OwnerId { get; set; }

        public User Owner { get; set; } = null!;

        public string Name { get; set; } = null!;

        public ICollection<SharedWalletMember> Members { get; set; }
            = new List<SharedWalletMember>();
    }
}
