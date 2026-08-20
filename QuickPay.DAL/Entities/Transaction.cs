using QuickPay.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Entities
{
    public class Transaction : BaseEntity
    {
        public int FromAccountId { get; set; }

        public FinancialAccount FromAccount { get; set; } = null!;

        public int ToAccountId { get; set; }

        public FinancialAccount ToAccount { get; set; } = null!;

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        public int? PaymentId { get; set; }

        public Payment? Payment { get; set; }

        public int? PaymentGatewayTransactionId { get; set; }

        public PaymentGatewayTransaction? PaymentGatewayTransaction { get; set; }
    }
}
