using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Configurations
{
    public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(EntityTypeBuilder<OtpCode> builder)
        {
            builder.ToTable("OtpCodes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CodeHash)
                .IsRequired();

            builder.Property(x => x.Purpose)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.IsUsed)
                .IsRequired();

            builder.Property(x => x.AttemptCount)
                .IsRequired();

            builder.Ignore(x => x.IsExpired);
        }
    }
}
