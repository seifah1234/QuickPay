using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Enums
{
    public enum PaymentGatewayTransactionStatus
    {
        Pending,
        Succeeded,
        Failed,
        Cancelled
    }
}
