using Microsoft.EntityFrameworkCore;

namespace QuickPay.DAL
{
    public class QuickPayDbContext : DbContext
    {

        public QuickPayDbContext(DbContextOptions<QuickPayDbContext> options) : base(options)
        {
        }


    }
}
