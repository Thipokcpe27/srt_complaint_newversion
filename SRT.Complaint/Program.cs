using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SRT.Complaint.Data;
using SRT.Complaint.Services;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/srt-complaint-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .WriteTo.Console()
              .WriteTo.File("logs/srt-complaint-.log", rollingInterval: RollingInterval.Day));

    // ──────────── Database ────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDbContext<CorruptionDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ──────────── Authentication & Authorization ────────────
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Cookies";
        options.DefaultSignInScheme = "Cookies";
        options.DefaultChallengeScheme = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Staff/Login";
        options.LogoutPath = "/Staff/Logout";
        options.AccessDeniedPath = "/Staff/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var jwtSecret = builder.Configuration["Jwt:Secret"] ?? string.Empty;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("GeneralOfficer", policy => policy.RequireRole("GeneralOfficer"));
        options.AddPolicy("CorruptionOfficer", policy => policy.RequireRole("CorruptionOfficer"));
        options.AddPolicy("StaffOnly", policy => policy.RequireRole("GeneralOfficer", "CorruptionOfficer", "SuperAdmin"));
        options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    });

    // ──────────── Rate Limiting ────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("SubmitPolicy", limiter =>
        {
            limiter.PermitLimit = 5;
            limiter.Window = TimeSpan.FromHours(1);
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiter.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // ──────────── Memory Cache (for API rate limiting) ────────────
    builder.Services.AddMemoryCache();

    // ──────────── Services ────────────
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IMaskingService, MaskingService>();
    builder.Services.AddScoped<ISlaService, SlaService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IComplaintService, ComplaintService>();
    builder.Services.AddScoped<ICorruptionService, CorruptionService>();
    builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
    builder.Services.AddScoped<IPdfExportService, PdfExportService>();

    // ──────────── Background Services ────────────
    builder.Services.AddHostedService<SlaBackgroundService>();

    // ──────────── MVC & Razor Pages ────────────
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();

    // ──────────── HTTP Clients ────────────
    builder.Services.AddHttpClient();

    // ──────────── File Upload ────────────
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 10 * 1024 * 1024 * 5; // 5 files × 10 MB
    });

    var app = builder.Build();

    // ──────────── Middleware Pipeline ────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}
