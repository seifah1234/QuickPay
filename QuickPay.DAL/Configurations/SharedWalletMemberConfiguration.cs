using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Configurations
{
    public class SharedWalletMemberConfiguration
    : IEntityTypeConfiguration<SharedWalletMember>
    {
        public void Configure(EntityTypeBuilder<SharedWalletMember> builder)
        {
            builder.ToTable("SharedWalletMembers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Role)
                .IsRequired();

            builder.Property(x => x.JoinedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.SharedWalletMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SharedWallet)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.SharedWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.SharedWalletId,
                x.UserId
            })
            .IsUnique();
        }
    }
}
