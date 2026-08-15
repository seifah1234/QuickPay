using Microsoft.EntityFrameworkCore;
using QuickPay.BLL.AutoMapper;
using QuickPay.BLL.Services.Implementation;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.DAL;
using QuickPay.DAL.Repositries.Implementaion;
using QuickPay.DAL.Repositries.Interfaces;
using QuickPay.DAL.UnitOfWork;
using QuickPay.PL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<QuickPayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionRepository,
    TransactionRepository>();

builder.Services.AddScoped<IFinancialAccountRepository,
    FinancialAccountRepository>();

builder.Services.AddScoped<IUnitOfWork,
    UnitOfWork>();

builder.Services.AddScoped<ITransferService,
    TransferService>();

builder.Services.AddScoped<IFinancialAccountService,
    FinancialAccountService>();

// TEMPORARY: stub current-user resolution until the Identity & Profile
// module (Phase 1, Member A) merges into develop. Swap CurrentUserService
// for a real ClaimsPrincipal-based implementation at that point -
// everything that depends on ICurrentUserService stays unchanged.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService,
    CurrentUserService>();

builder.Services.AddAutoMapper(m => m.AddProfile<TransferProfile>());

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


app.Run();
