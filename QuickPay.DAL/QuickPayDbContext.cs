using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entities;

namespace QuickPay.DAL
{
    public class QuickPayDbContext : DbContext
    {

        public QuickPayDbContext(DbContextOptions<QuickPayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        public DbSet<OtpCode> OtpCodes { get; set; } = null!;

        public DbSet<FinancialAccount> FinancialAccounts { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<SharedWallet> SharedWallets { get; set; }

        public DbSet<SharedWalletMember> SharedWalletMembers { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<PaymentGatewayTransaction> PaymentGatewayTransactions { get; set; }

        public DbSet<SplitGroup> SplitGroups { get; set; }

        public DbSet<SplitParticipant> SplitParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(QuickPayDbContext).Assembly);
        }

    }
}
