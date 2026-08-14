using Microsoft.EntityFrameworkCore;
using QuickPay.BLL.Services.Abstraction;
using QuickPay.BLL.Services.Implementation;
using QuickPay.DAL.Database;
using QuickPay.DAL.Repo.Abstraction;
using QuickPay.DAL.Repo.Implementation;
using QuickPay.PL.Hubs;

namespace QuickPay.PL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            builder.Services.AddDbContext<ProjectDBContext>(op => op.UseSqlServer(builder.Configuration.GetConnectionString("ProjectCon")));

            builder.Services.AddScoped<INotificationRepo, NotificationRepo>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
            builder.Services.AddScoped<ITransactionHistoryRepo, TransactionHistoryRepo>();
            builder.Services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapHub<NotificationHub>("/hubs/notifications");

            app.Run();
        }
    }
}
