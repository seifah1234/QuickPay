using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Configurations
{
    public class FinancialAccountConfiguration
    : IEntityTypeConfiguration<FinancialAccount>
    {
        public void Configure(EntityTypeBuilder<FinancialAccount> builder)
        {
            builder.ToTable("FinancialAccounts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Balance)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            builder.HasDiscriminator<string>("AccountType")
                .HasValue<Wallet>("Wallet")
                .HasValue<SharedWallet>("SharedWallet");
        }
    }
}
