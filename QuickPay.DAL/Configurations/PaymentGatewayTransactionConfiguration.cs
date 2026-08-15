using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Configurations
{
    public class PaymentGatewayTransactionConfiguration
    : IEntityTypeConfiguration<PaymentGatewayTransaction>
    {
        public void Configure(
            EntityTypeBuilder<PaymentGatewayTransaction> builder)
        {
            builder.ToTable("PaymentGatewayTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.GatewayTransactionId)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.GatewayTransactionId)
                .IsUnique();
        }
    }
}
