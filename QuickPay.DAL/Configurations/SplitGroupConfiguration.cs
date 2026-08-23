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

            // DueDate is optional (DateTime? on the entity) - a required
            // NOT NULL column here would force every split to have a due
            // date, contradicting the CLR type. Nullable columns are
            // optional by convention, so no explicit call is needed.
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
