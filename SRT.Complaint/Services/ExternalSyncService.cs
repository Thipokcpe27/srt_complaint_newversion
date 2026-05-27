#nullable enable
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class ExternalSyncService(
    IEnumerable<IExternalSystemAdapter> adapters,
    AppDbContext db,
    IComplaintService complaintService,
    ILogger<ExternalSyncService> logger) : IExternalSyncService
{
    private readonly IReadOnlyList<IExternalSystemAdapter> _adapters = adapters.ToList();

    public IReadOnlyList<IExternalSystemAdapter> GetAvailableSystems() => _adapters;

    public async Task<ExternalSyncLog> SyncAsync(string systemKey, int triggeredById, CancellationToken ct = default)
    {
        var adapter = _adapters.FirstOrDefault(a => a.SystemKey == systemKey)
            ?? throw new InvalidOperationException($"ไม่พบระบบ '{systemKey}'");

        var log = new ExternalSyncLog
        {
            ExternalSystem = systemKey,
            StartedAt      = DateTime.UtcNow,
            SyncStatus     = "Running",
            TriggeredById  = triggeredById
        };
        db.ExternalSyncLogs.Add(log);
        await db.SaveChangesAsync(ct);

        try
        {
            if (!adapter.IsConfigured)
                throw new InvalidOperationException($"{adapter.DisplayName} ยังไม่ได้ตั้งค่า API — กรุณาใส่ค่าใน appsettings");

            var result = await adapter.FetchNewAsync(ct);

            foreach (var item in result.Items)
            {
                log.FetchedCount++;
                try
                {
                    var exists = await db.Complaints
                        .AnyAsync(c => c.ExternalSystem == systemKey && c.ExternalId == item.ExternalId, ct);

                    if (exists) { log.DuplicateCount++; continue; }

                    await ImportComplaintAsync(item, systemKey, ct);
                    log.NewCount++;
                }
                catch (Exception ex)
                {
                    log.ErrorCount++;
                    logger.LogWarning(ex, "Failed to import {ExternalId} from {System}", item.ExternalId, systemKey);
                }
            }

            log.SyncStatus    = "Success";
            log.CompletedAt   = DateTime.UtcNow;
            log.ErrorMessage  = result.ErrorMessage;
        }
        catch (Exception ex)
        {
            log.SyncStatus   = "Failed";
            log.CompletedAt  = DateTime.UtcNow;
            log.ErrorMessage = ex.Message;
            logger.LogError(ex, "Sync failed for {System}", systemKey);
        }

        await db.SaveChangesAsync(ct);
        return log;
    }

    public async Task<IReadOnlyList<ExternalSyncLog>> GetRecentLogsAsync(int count = 20, CancellationToken ct = default)
        => await db.ExternalSyncLogs
            .Include(l => l.TriggeredBy)
            .OrderByDescending(l => l.StartedAt)
            .Take(count)
            .ToListAsync(ct);

    private async Task ImportComplaintAsync(ExternalComplaintDto item, string systemKey, CancellationToken ct)
    {
        // Map category: ใช้หมวด "อื่น ๆ" เป็น default ก่อน
        // เมื่อได้ CategoryMapping จากแต่ละ adapter ค่อย map จริง
        var defaultCategory = await db.ComplaintCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .FirstOrDefaultAsync(ct) ?? throw new InvalidOperationException("ไม่มีหมวดหมู่เรื่องร้องเรียน");

        var request = new SubmitComplaintRequest(
            ReporterName:   item.ReporterName ?? $"นำเข้าจาก {systemKey}",
            ReporterPhone:  item.ReporterPhone ?? "-",
            ReporterEmail:  null,
            ReporterIdCard: null,
            CategoryId:     defaultCategory.Id,
            SubCategoryId:  null,
            SubjectStation: item.Address,
            IncidentDate:   item.IncidentDate.HasValue ? DateOnly.FromDateTime(item.IncidentDate.Value) : null,
            Description:    item.Description,
            Attachments:    Array.Empty<IFormFile>()
        );

        var complaint = await complaintService.SubmitAsync(request, ct);

        // ติด ExternalId และ Channel
        complaint.ExternalSystem = systemKey;
        complaint.ExternalId     = item.ExternalId;
        complaint.Channel        = systemKey;
        await db.SaveChangesAsync(ct);
    }
}
