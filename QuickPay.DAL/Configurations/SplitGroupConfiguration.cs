using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Configurations
{
    public class SplitGroupConfiguration
    : IEntityTypeConfiguration<SplitGroup>
    {
        public void Configure(EntityTypeBuilder<SplitGroup> builder)
        {
            builder.ToTable("SplitGroups");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.SplitType)
                .IsRequired();

            builder.Property(x => x.DueDate)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Payment)
                .WithOne(x => x.SplitGroup)
                .HasForeignKey<SplitGroup>(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Participants)
                .WithOne(x => x.SplitGroup)
                .HasForeignKey(x => x.SplitGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PaymentId)
                .IsUnique();
        }
    }
}
