using Microsoft.EntityFrameworkCore;
using QuickPay.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPay.DAL.Database
{
    public class ProjectDBContext:DbContext
    {

        public ProjectDBContext() { }

        public ProjectDBContext(DbContextOptions<ProjectDBContext> options) : base(options) { }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }
}
