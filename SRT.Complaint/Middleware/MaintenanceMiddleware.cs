using Microsoft.Extensions.Caching.Memory;
using SRT.Complaint.Services;

namespace SRT.Complaint.Middleware;

public class MaintenanceMiddleware(
    RequestDelegate next,
    IMemoryCache cache,
    IServiceScopeFactory scopeFactory)
{
    private static readonly string[] _allowedPrefixes =
    [
        "/Staff", "/Admin", "/Corruption", "/api",
        "/css", "/js", "/lib", "/images", "/fonts",
        "/favicon", "/Maintenance"
    ];

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (!await IsMaintenanceModeAsync())
        {
            await next(ctx);
            return;
        }

        var path = ctx.Request.Path.Value ?? "";

        // เจ้าหน้าที่ที่ login แล้วผ่านได้เสมอ
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            await next(ctx);
            return;
        }

        // Static files และ path ที่อนุญาต
        if (_allowedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(ctx);
            return;
        }

        ctx.Response.Redirect("/Maintenance");
    }

    private async Task<bool> IsMaintenanceModeAsync()
    {
        const string cacheKey = "sys:maintenance.enabled";
        if (cache.TryGetValue(cacheKey, out bool cached))
            return cached;

        try
        {
            using var scope = scopeFactory.CreateScope();
            var settings = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();
            var value = await settings.GetAsync("maintenance.enabled", "false");
            var enabled = value == "true";

            cache.Set(cacheKey, enabled, TimeSpan.FromSeconds(30));
            return enabled;
        }
        catch
        {
            // DB ไม่พร้อม → fail open (ให้ traffic ผ่าน) และ cache ค่า false 10 วินาที
            // เพื่อกัน thundering herd กับ DB ที่กำลังล่ม
            cache.Set(cacheKey, false, TimeSpan.FromSeconds(10));
            return false;
        }
    }
}
