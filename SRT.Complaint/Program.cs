using System.Security.Cryptography;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using SRT.Complaint.Data;
using SRT.Complaint.Filters;
using SRT.Complaint.Services;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/srt-complaint-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.local.json",
        optional: true, reloadOnChange: true);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .WriteTo.Console()
              .WriteTo.File("logs/srt-complaint-.log", rollingInterval: RollingInterval.Day));

    // ──────────── Database ────────────
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connStr)
               .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

    builder.Services.AddDbContext<CorruptionDbContext>(options =>
        options.UseSqlServer(connStr)
               .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

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
        options.ExpireTimeSpan = TimeSpan.FromMinutes(
            builder.Configuration.GetValue("Security:SessionTimeoutMinutes", 30));
        options.SlidingExpiration = true;
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("GeneralOfficer", policy => policy.RequireRole("GeneralOfficer"));
        options.AddPolicy("CorruptionOfficer", policy => policy.RequireRole("CorruptionOfficer"));
        options.AddPolicy("StaffOnly", policy => policy.RequireRole("GeneralOfficer", "CorruptionOfficer", "SuperAdmin"));
        options.AddPolicy("CorruptionAccess", policy => policy.RequireRole("CorruptionOfficer", "SuperAdmin"));
        options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    });

    // ──────────── Rate Limiting (อ่านจาก appsettings.json → Security.*) ────────────
    builder.Services.AddRateLimiter(options =>
    {
        var sec = builder.Configuration.GetSection("Security");
        options.AddFixedWindowLimiter("SubmitPolicy", limiter =>
        {
            limiter.PermitLimit = sec.GetValue("SubmitLimitPerHour", 5);
            limiter.Window = TimeSpan.FromHours(1);
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiter.QueueLimit = 0;
        });
        options.AddFixedWindowLimiter("TrackVerifyPolicy", limiter =>
        {
            limiter.PermitLimit = sec.GetValue("TrackVerifyLimitPerWindow", 10);
            limiter.Window = TimeSpan.FromMinutes(sec.GetValue("TrackVerifyWindowMinutes", 15));
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiter.QueueLimit = 0;
        });
        options.AddFixedWindowLimiter("LoginPolicy", limiter =>
        {
            limiter.PermitLimit = sec.GetValue("LoginLimitPerWindow", 10);
            limiter.Window = TimeSpan.FromMinutes(sec.GetValue("LoginWindowMinutes", 15));
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiter.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // ──────────── Memory Cache (for API rate limiting) ────────────
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(
            builder.Configuration.GetValue("Security:SessionTimeoutMinutes", 30));
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    // ──────────── Services ────────────
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IMaskingService, MaskingService>();
    builder.Services.AddScoped<ISlaService, SlaService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IComplaintService, ComplaintService>();
    builder.Services.AddScoped<ICorruptionService, CorruptionService>();
    builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
    builder.Services.AddScoped<IPdfExportService, PdfExportService>();
    builder.Services.AddScoped<IApiRequestLogService, ApiRequestLogService>();
    builder.Services.AddScoped<IWebhookService, WebhookService>();
    builder.Services.AddScoped<ITermsService, TermsService>();
    builder.Services.AddScoped<IContentBlockService, ContentBlockService>();
    builder.Services.AddScoped<ITurnstileService, TurnstileService>();
    builder.Services.AddScoped<IStatsService, StatsService>();
    builder.Services.AddScoped<ICorruptionStatsService, CorruptionStatsService>();
    builder.Services.AddScoped<IExternalSyncService, ExternalSyncService>();
    builder.Services.AddScoped<IFaqService, FaqService>();
    builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
    builder.Services.AddSingleton<IExternalSystemAdapter, SRT.Complaint.Services.Adapters.TraffyFonduAdapter>();
    builder.Services.AddSingleton<IExternalSystemAdapter, SRT.Complaint.Services.Adapters.DamrongdhammaAdapter>();
    builder.Services.AddScoped<ApiKeyAuthFilter>();

    // ──────────── Background Services ────────────
    builder.Services.AddHostedService<SlaBackgroundService>();
    builder.Services.AddHostedService<WebhookRetryService>();
    builder.Services.AddHostedService<AuditLogRetentionService>();
    builder.Services.AddHostedService<PdpaRetentionService>();

    // ──────────── MVC & Razor Pages ────────────
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();

    // ──────────── HTTP Clients ────────────
    builder.Services.AddHttpClient();
    builder.Services.AddHttpClient("Webhook", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });
    builder.Services.AddHttpClient("Sms", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    // ──────────── File Upload ────────────
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 10 * 1024 * 1024 * 5; // 5 files × 10 MB
    });

    var app = builder.Build();

    // ──────────── Startup validation ────────────
    var encryptionKey = builder.Configuration["Encryption:Key"];
    if (string.IsNullOrWhiteSpace(encryptionKey))
        throw new InvalidOperationException(
            "Encryption:Key ยังไม่ได้ตั้งค่า — กำหนด Environment Variable 'Encryption__Key' ใน IIS ก่อน deploy");

    // ──────────── Seed SuperAdmin (first-run only) ────────────
    var tempAdminPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!db.StaffUsers.Any(u => u.Role == "SuperAdmin"))
        {
            db.StaffUsers.Add(new SRT.Complaint.Models.StaffUser
            {
                EmployeeCode = "0000001",
                FullName = "Super Administrator",
                Email = "admin@railway.co.th",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempAdminPassword, 12),
                Role = "SuperAdmin",
                IsActive = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
            Log.Warning("⚠️  SuperAdmin สร้างครั้งแรก — รหัสผ่านชั่วคราว (ใช้ได้ครั้งเดียว): {Pass} — เปลี่ยนทันทีหลัง login", tempAdminPassword);
        }
    }

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

    app.UseSerilogRequestLogging(opts =>
    {
        // ซ่อน token ที่อยู่ใน path ของ traffy-webhook
        // /api/traffy-webhook/{token}/new-issue → /api/traffy-webhook/[redacted]/new-issue
        opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
        {
            var path = httpCtx.Request.Path.Value ?? "";
            if (path.StartsWith("/api/traffy-webhook/", StringComparison.OrdinalIgnoreCase))
            {
                var segments = path.TrimStart('/').Split('/');
                if (segments.Length >= 3) segments[2] = "[redacted]";
                diagCtx.Set("RequestPath", "/" + string.Join("/", segments));
            }
        };
    });
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseMiddleware<SRT.Complaint.Middleware.MaintenanceMiddleware>();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseSession();
    app.UseAuthentication();

    // Redirect to password change page if MustChangePassword claim is set
    app.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.HasClaim("MustChangePassword", "true"))
        {
            var path = context.Request.Path.Value ?? "";
            var isAllowed = path.StartsWith("/Staff/ChangePassword", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("/Staff/Logout", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase);
            if (!isAllowed)
            {
                context.Response.Redirect("/Staff/ChangePassword");
                return;
            }
        }
        await next(context);
    });

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
