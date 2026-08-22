using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuickPay.BLL.AutoMapper;
using QuickPay.BLL.Services.Implementation;
using QuickPay.BLL.Services.Interfaces;
using QuickPay.BLL.Services.SplitStrategies;
using QuickPay.BLL.Settings;
using QuickPay.DAL;
using QuickPay.DAL.Repositries.Implementaion;
using QuickPay.DAL.Repositries.Interfaces;
using QuickPay.DAL.UnitOfWork;
using QuickPay.PL.Hubs;
using QuickPay.PL.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuickPayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

LoadDotEnvInto(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();


builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IFinancialAccountRepository, FinancialAccountRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISplitGroupRepository, SplitGroupRepository>();
builder.Services.AddScoped<ISplitParticipantRepository, SplitParticipantRepository>();
builder.Services.AddScoped<ISharedWalletRepository, SharedWalletRepository>();
builder.Services.AddScoped<ISharedWalletMemberRepository, SharedWalletMemberRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<IOtpDeliveryService, LogOtpDeliveryService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFinancialAccountService, FinancialAccountService>();
builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ISmartSplitService, SmartSplitService>();
builder.Services.AddScoped<ISplitStrategy, EqualSplitStrategy>();
builder.Services.AddScoped<ISplitStrategy, CustomAmountSplitStrategy>();
builder.Services.AddScoped<ISplitStrategy, PercentageSplitStrategy>();
builder.Services.AddScoped<ISplitStrategyFactory, SplitStrategyFactory>();
builder.Services.AddScoped<ISharedWalletService, SharedWalletService>();

builder.Services.AddAutoMapper(m => m.AddProfile<AuthProfile>());
builder.Services.AddAutoMapper(m => m.AddProfile<TransferProfile>());

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? new JwtSettings();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}")
    .WithStaticAssets();

app.MapHub<NotificationHub>("/hubs/notifications");


app.Run();

static void LoadDotEnvInto(ConfigurationManager configuration, string contentRootPath)
{
    var envPath = Path.Combine(contentRootPath, ".env");

    if (!File.Exists(envPath))
    {
        return;
    }

    foreach (var rawLine in File.ReadAllLines(envPath))
    {
        var line = rawLine.Trim();

        if (line.Length == 0 || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');

        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim().Trim('"');

        if (value.Length == 0)
        {
            continue;
        }

        configuration[key.Replace("__", ":")] = value;
    }
}