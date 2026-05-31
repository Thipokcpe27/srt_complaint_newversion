using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;

namespace SRT.Complaint.Services;

public class AuditLogRetentionService(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<AuditLogRetentionService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // รอ 5 นาทีหลัง startup แล้วค่อยทำงาน ไม่กระทบ boot time
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await PurgeOldLogsAsync(stoppingToken);
            // รันซ้ำทุก 24 ชั่วโมง
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task PurgeOldLogsAsync(CancellationToken ct)
    {
        // อ่านจาก SystemSettings DB ก่อน fallback ไป appsettings.json
        int retentionDays;
        try
        {
            using var scope = scopeFactory.CreateScope();
            var settingService = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();
            var raw = await settingService.GetAsync("audit.retention_days");
            retentionDays = int.TryParse(raw, out var d) && d >= 90 ? d : config.GetValue("AuditLog:RetentionDays", 90);
        }
        catch
        {
            retentionDays = config.GetValue("AuditLog:RetentionDays", 90);
        }

        if (retentionDays <= 0) return;

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var deleted = await db.AuditLogs
                .Where(a => a.CreatedAt < cutoff)
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
                logger.LogInformation("AuditLogRetention: ลบ {Count} รายการที่เก่ากว่า {Days} วัน", deleted, retentionDays);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "AuditLogRetention: เกิดข้อผิดพลาดขณะลบ log เก่า");
        }
    }
}
