using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickPay.DAL.Entities;

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

            builder.Property(x => x.ProviderTransactionId)
                .HasMaxLength(200);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Direction)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Same GatewayTransactionId can never be processed twice -
            // this unique index IS the idempotency guard on webhook
            // replays/duplicate deliveries.
            builder.HasIndex(x => x.GatewayTransactionId)
                .IsUnique();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Wallet)
                .WithMany()
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BankAccount)
                .WithMany()
                .HasForeignKey(x => x.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
