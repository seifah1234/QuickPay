using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                .IsRequired();

            builder.HasIndex(x => x.Token)
                .IsUnique();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Ignore(x => x.IsActive);
        }
    }
}
