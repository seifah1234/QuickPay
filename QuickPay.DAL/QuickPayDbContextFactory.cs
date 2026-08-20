using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QuickPay.DAL
{
    public class QuickPayDbContextFactory : IDesignTimeDbContextFactory<QuickPayDbContext>
    {
        public QuickPayDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QuickPayDbContext>();
            
            // Force-load the .env file explicitly for the design-time process
            DotNetEnv.Env.Load(); // Or dotenv.net's DotEnv.Load();

            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
            // 4. Build the DbContext options
            optionsBuilder.UseSqlServer(connectionString);

            //optionsBuilder.UseSqlServer(
              //  "Server=localhost;Database=QuickPayDB;Trusted_Connection=true;TrustServerCertificate=true;");

            return new QuickPayDbContext(optionsBuilder.Options);
        }
    }
}