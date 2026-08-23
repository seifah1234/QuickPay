using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Enums
{
    public enum TransactionType
    {
        Transfer,
        Deposit,
        Withdraw,
        Payment,
        SplitPayment
    }
}
