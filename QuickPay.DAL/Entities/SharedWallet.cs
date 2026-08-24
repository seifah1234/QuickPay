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

        public async Task<User> WalletOwner()
        {
            return await Task.FromResult(Members.FirstOrDefault(m => m.Role == Enums.SharedWalletRole.Admin)?.User
                ?? throw new InvalidOperationException("Shared wallet must have an owner."));
        } 
    }
}
