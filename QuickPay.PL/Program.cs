using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
using System.Globalization;
using System.Text;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuickPayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

LoadDotEnvInto(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "QuickPay API",
        Version = "v1",
        Description = "Webhook and API endpoints for the QuickPay digital payment platform."
    });

    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste a raw JWT access token (no \"Bearer \" prefix needed here)."
    };

    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});


builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<PaymobSettings>(
    builder.Configuration.GetSection("Paymob"));

builder.Services.Configure<TwilioSettings>(
    builder.Configuration.GetSection("Twilio"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IFinancialAccountRepository, FinancialAccountRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();

builder.Services.AddScoped<IPaymentGatewayTransactionRepository, PaymentGatewayTransactionRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISplitGroupRepository, SplitGroupRepository>();
builder.Services.AddScoped<ISplitParticipantRepository, SplitParticipantRepository>();
builder.Services.AddScoped<ISharedWalletRepository, SharedWalletRepository>();
builder.Services.AddScoped<ISharedWalletMemberRepository, SharedWalletMemberRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<IOtpDeliveryService, TwilioOtpDeliveryService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFinancialAccountService, FinancialAccountService>();
builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ILinkedAccountsService, LinkedAccountsService>();
builder.Services.AddScoped<IPaymentGatewayProvider, PaymobGatewayProvider>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();



builder.Services.AddHttpClient<IPaymentGatewayProvider, PaymobGatewayProvider>();
builder.Services.AddScoped<ISmartSplitService, SmartSplitService>();
builder.Services.AddScoped<ISplitStrategy, EqualSplitStrategy>();
builder.Services.AddScoped<ISplitStrategy, CustomAmountSplitStrategy>();
builder.Services.AddScoped<ISplitStrategy, PercentageSplitStrategy>();
builder.Services.AddScoped<ISplitStrategyFactory, SplitStrategyFactory>();
builder.Services.AddScoped<ISharedWalletService, SharedWalletService>();

builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

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
    })
    .AddCookie("ExternalCookie", options =>
    {
        options.Cookie.Name = "ExternalAuthTempToken";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddGoogle(options =>
    {
        options.SignInScheme = "ExternalCookie";
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickPay API v1");
    });
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

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