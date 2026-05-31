#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class PdpaRetentionModel(
    AppDbContext appDb,
    CorruptionDbContext corrDb,
    ISystemSettingService settings,
    IServiceScopeFactory scopeFactory,
    ILogger<PdpaRetentionModel> logger,
    IAuditService auditService) : PageModel
{
    // ── Stats ───────────────────────────────────────────────
    public int ComplaintPiiDeletedCount   { get; private set; }
    public int ComplaintPiiPendingCount   { get; private set; }
    public int CorruptionPiiDeletedCount  { get; private set; }
    public int CorruptionPiiPendingCount  { get; private set; }

    // ── Settings ────────────────────────────────────────────
    [BindProperty] public int ComplaintRetentionDays   { get; set; } = 1825;
    [BindProperty] public int CorruptionRetentionDays  { get; set; } = 1825;

    // ── Recent purges ───────────────────────────────────────
    public IReadOnlyList<PurgedRow> RecentPurged { get; private set; } = [];

    public record PurgedRow(string Ref, string Type, DateTime PiiDeletedAt, DateTime? ClosedAt);

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var days1 = Math.Max(365, ComplaintRetentionDays);
        var days2 = Math.Max(365, CorruptionRetentionDays);

        await settings.SaveGroupAsync("pdpa", new()
        {
            ["pdpa.complaint_retention_days"]  = days1.ToString(),
            ["pdpa.corruption_retention_days"] = days2.ToString(),
        }, UserId());

        await auditService.LogAsync("UpdatePdpaSettings", UserId(), GetActorCode(),
            "SystemSetting", "pdpa",
            new { complaintRetentionDays = days1, corruptionRetentionDays = days2 },
            GetIp(), outcome: "Success");
        TempData["Success"] = "บันทึกการตั้งค่า PDPA เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRunNowAsync()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            await PdpaRetentionService.RunNowAsync(scope, logger);
            await auditService.LogAsync("PdpaManualRun", UserId(), GetActorCode(),
                "SystemSetting", null, new { trigger = "manual" }, GetIp(), outcome: "Success");
            TempData["Success"] = "ลบข้อมูลส่วนตัวที่ครบกำหนดเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PDPA manual run failed");
            TempData["Error"] = "เกิดข้อผิดพลาด: " + ex.Message;
        }
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var pdpa = await settings.GetGroupAsync("pdpa");
        ComplaintRetentionDays  = int.TryParse(pdpa.GetValueOrDefault("pdpa.complaint_retention_days",  "1825"), out var d1) ? d1 : 1825;
        CorruptionRetentionDays = int.TryParse(pdpa.GetValueOrDefault("pdpa.corruption_retention_days", "1825"), out var d2) ? d2 : 1825;

        var complaintCutoff   = DateTime.UtcNow.AddDays(-ComplaintRetentionDays);
        var corruptionCutoff  = DateTime.UtcNow.AddDays(-CorruptionRetentionDays);

        ComplaintPiiDeletedCount  = await appDb.Complaints.CountAsync(c => c.PiiDeletedAt != null);
        ComplaintPiiPendingCount  = await appDb.Complaints.CountAsync(c =>
            c.PiiDeletedAt == null &&
            (c.ClosedAt.HasValue ? c.ClosedAt < complaintCutoff : c.CreatedAt < complaintCutoff));

        CorruptionPiiDeletedCount = await corrDb.Reports.CountAsync(r => r.PiiDeletedAt != null);
        CorruptionPiiPendingCount = await corrDb.Reports.CountAsync(r =>
            r.PiiDeletedAt == null &&
            (r.ClosedAt.HasValue ? r.ClosedAt < corruptionCutoff : r.CreatedAt < corruptionCutoff));

        // recent purges — complaints
        var recentComplaints = await appDb.Complaints
            .Where(c => c.PiiDeletedAt != null)
            .OrderByDescending(c => c.PiiDeletedAt)
            .Take(10)
            .Select(c => new PurgedRow(c.ReferenceNumber, "เรื่องร้องเรียน", c.PiiDeletedAt!.Value, c.ClosedAt))
            .ToListAsync();

        var recentCorruption = await corrDb.Reports
            .Where(r => r.PiiDeletedAt != null)
            .OrderByDescending(r => r.PiiDeletedAt)
            .Take(10)
            .Select(r => new PurgedRow(r.ReferenceNumber, "เรื่องทุจริต", r.PiiDeletedAt!.Value, r.ClosedAt))
            .ToListAsync();

        RecentPurged = recentComplaints.Concat(recentCorruption)
            .OrderByDescending(r => r.PiiDeletedAt)
            .Take(15)
            .ToList();
    }

    private int    UserId()       => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetActorCode() => User.FindFirstValue("EmployeeCode") ?? "";
    private string GetIp()        => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
