using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuickPay.DAL
{
    public class QuickPayDbContextFactory : IDesignTimeDbContextFactory<QuickPayDbContext>
    {
        public QuickPayDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QuickPayDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=QuickPayDB;Trusted_Connection=true;TrustServerCertificate=true;");

            return new QuickPayDbContext(optionsBuilder.Options);
        }
    }
}