using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Configurations
{
    public class TransactionConfiguration
    : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // From Account
            builder.HasOne(x => x.FromAccount)
                .WithMany(x => x.OutgoingTransactions)
                .HasForeignKey(x => x.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // To Account
            builder.HasOne(x => x.ToAccount)
                .WithMany(x => x.IncomingTransactions)
                .HasForeignKey(x => x.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment - Nullable
            builder.HasOne(x => x.Payment)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment Gateway Transaction
            builder.HasOne(x => x.PaymentGatewayTransaction)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.PaymentGatewayTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
