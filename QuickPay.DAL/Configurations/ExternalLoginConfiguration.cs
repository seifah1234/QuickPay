using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;

namespace QuickPay.DAL.Configurations
{
    public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
    {
        public void Configure(EntityTypeBuilder<ExternalLogin> builder)
        {
            builder.ToTable("ExternalLogins");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Provider)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ProviderUserId)
                .IsRequired();
            
            builder.HasIndex(e => new { e.UserId, e.ProviderUserId })
                .IsUnique();

            // Many ExternalLogins -> One User
            builder.HasOne(x => x.User)
                .WithMany(x => x.ExternalLogins)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}