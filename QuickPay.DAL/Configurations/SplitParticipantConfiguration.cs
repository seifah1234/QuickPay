using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Configurations
{
    public class SplitParticipantConfiguration
    : IEntityTypeConfiguration<SplitParticipant>
    {
        public void Configure(EntityTypeBuilder<SplitParticipant> builder)
        {
            builder.ToTable("SplitParticipants");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Percentage)
                .HasPrecision(5, 2);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SplitGroup)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.SplitGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.SplitGroupId,
                x.UserId
            })
            .IsUnique();
        }
    }
}
