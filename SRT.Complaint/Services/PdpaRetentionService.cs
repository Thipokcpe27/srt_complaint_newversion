using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;

namespace SRT.Complaint.Services;

public class PdpaRetentionService(IServiceScopeFactory scopeFactory, ILogger<PdpaRetentionService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(7), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await PurgeComplaintPiiAsync(stoppingToken);
            await PurgeCorruptionPiiAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task PurgeComplaintPiiAsync(CancellationToken ct)
    {
        var days = await GetRetentionDaysAsync("pdpa.complaint_retention_days", 1825);
        var cutoff = DateTime.UtcNow.AddDays(-days);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var targets = await db.Complaints
                .Where(c => c.PiiDeletedAt == null
                    && (c.ClosedAt.HasValue ? c.ClosedAt < cutoff : c.CreatedAt < cutoff))
                .ToListAsync(ct);

            if (targets.Count == 0) return;

            foreach (var c in targets)
            {
                c.ReporterName   = "[ลบตาม PDPA]";
                c.ReporterPhone  = "";
                c.ReporterEmail  = null;
                c.ReporterIdCard = null;
                c.PiiDeletedAt   = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("PDPA: anonymized PII ของ {Count} เรื่องร้องเรียน (เกิน {Days} วัน)", targets.Count, days);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "PDPA: เกิดข้อผิดพลาดขณะลบ PII เรื่องร้องเรียน");
        }
    }

    private async Task PurgeCorruptionPiiAsync(CancellationToken ct)
    {
        var days = await GetRetentionDaysAsync("pdpa.corruption_retention_days", 1825);
        var cutoff = DateTime.UtcNow.AddDays(-days);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CorruptionDbContext>();

            var targets = await db.Reports
                .Where(r => r.PiiDeletedAt == null
                    && (r.ClosedAt.HasValue ? r.ClosedAt < cutoff : r.CreatedAt < cutoff))
                .ToListAsync(ct);

            if (targets.Count == 0) return;

            foreach (var r in targets)
            {
                r.ReporterNameEncrypted  = Array.Empty<byte>();
                r.ReporterPhoneEncrypted = Array.Empty<byte>();
                r.ReporterEmailEncrypted = null;
                r.ReporterIdCardEncrypted = Array.Empty<byte>();
                r.ReporterNameMasked     = "[ลบตาม PDPA]";
                r.ReporterPhoneMasked    = "";
                r.ReporterEmailMasked    = null;
                r.PiiDeletedAt           = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("PDPA: anonymized PII ของ {Count} รายงานทุจริต (เกิน {Days} วัน)", targets.Count, days);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "PDPA: เกิดข้อผิดพลาดขณะลบ PII รายงานทุจริต");
        }
    }

    private async Task<int> GetRetentionDaysAsync(string dbKey, int defaultDays)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();
            var raw = await svc.GetAsync(dbKey);
            if (int.TryParse(raw, out var d) && d > 0) return d;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "PDPA: ไม่สามารถอ่าน retention days จาก DB (key: {Key}) ใช้ค่า default {Default} วัน", dbKey, defaultDays);
        }
        return defaultDays;
    }

    // ── Public method สำหรับ manual trigger จาก Admin UI ────────────────────
    public static async Task RunNowAsync(IServiceScope scope, ILogger logger, CancellationToken ct = default)
    {
        var appDb  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var corrDb = scope.ServiceProvider.GetRequiredService<CorruptionDbContext>();
        var svc    = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();

        async Task<int> GetDays(string key, int def)
        {
            var raw = await svc.GetAsync(key);
            return int.TryParse(raw, out var d) && d > 0 ? d : def;
        }

        var complaintDays   = await GetDays("pdpa.complaint_retention_days", 1825);
        var corruptionDays  = await GetDays("pdpa.corruption_retention_days", 1825);
        var complaintCutoff = DateTime.UtcNow.AddDays(-complaintDays);
        var corruptionCutoff = DateTime.UtcNow.AddDays(-corruptionDays);

        // Complaints
        var complaints = await appDb.Complaints
            .Where(c => c.PiiDeletedAt == null
                && (c.ClosedAt.HasValue ? c.ClosedAt < complaintCutoff : c.CreatedAt < complaintCutoff))
            .ToListAsync(ct);

        foreach (var c in complaints)
        {
            c.ReporterName   = "[ลบตาม PDPA]";
            c.ReporterPhone  = "";
            c.ReporterEmail  = null;
            c.ReporterIdCard = null;
            c.PiiDeletedAt   = DateTime.UtcNow;
        }
        if (complaints.Count > 0) await appDb.SaveChangesAsync(ct);

        // Corruption reports
        var reports = await corrDb.Reports
            .Where(r => r.PiiDeletedAt == null
                && (r.ClosedAt.HasValue ? r.ClosedAt < corruptionCutoff : r.CreatedAt < corruptionCutoff))
            .ToListAsync(ct);

        foreach (var r in reports)
        {
            r.ReporterNameEncrypted   = Array.Empty<byte>();
            r.ReporterPhoneEncrypted  = Array.Empty<byte>();
            r.ReporterEmailEncrypted  = null;
            r.ReporterIdCardEncrypted = Array.Empty<byte>();
            r.ReporterNameMasked      = "[ลบตาม PDPA]";
            r.ReporterPhoneMasked     = "";
            r.ReporterEmailMasked     = null;
            r.PiiDeletedAt            = DateTime.UtcNow;
        }
        if (reports.Count > 0) await corrDb.SaveChangesAsync(ct);

        logger.LogInformation("PDPA manual run: {C} complaints, {R} corruption reports anonymized",
            complaints.Count, reports.Count);
    }
}
