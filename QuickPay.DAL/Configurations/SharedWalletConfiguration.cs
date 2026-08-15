using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Configurations
{
    public class SharedWalletConfiguration
    : IEntityTypeConfiguration<SharedWallet>
    {
        public void Configure(EntityTypeBuilder<SharedWallet> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();


            builder.HasMany(x => x.Members)
                .WithOne(x => x.SharedWallet)
                .HasForeignKey(x => x.SharedWalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
