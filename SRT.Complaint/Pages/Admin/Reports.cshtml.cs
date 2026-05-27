#nullable enable
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class ReportsModel(AppDbContext db) : PageModel
{
    public IReadOnlyList<CategoryStat>  ByCategorySeries { get; private set; } = [];
    public IReadOnlyList<StatusStat>    ByStatusSeries   { get; private set; } = [];
    public List<string>                 MonthlyLabels    { get; private set; } = [];
    public List<int>                    MonthlyData      { get; private set; } = [];
    public IReadOnlyList<StaffStat>     TopStaff         { get; private set; } = [];
    public int TotalAll     { get; private set; }
    public int TotalClosed  { get; private set; }
    public int TotalOpen    { get; private set; }
    public int TotalBreached { get; private set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "รายงาน";
        await LoadStatsAsync();
    }

    // ─── Excel Export ───
    public async Task<IActionResult> OnPostExportAsync()
    {
        var complaints = await db.Complaints
            .Include(c => c.Category)
            .Include(c => c.AssignedTo)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5000)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("เรื่องร้องเรียน");

        var headers = new[]
        {
            "เลขที่อ้างอิง", "หมวดหมู่", "สถานะ", "ความสำคัญ",
            "ผู้ร้องเรียน", "โทรศัพท์", "ผู้รับผิดชอบ",
            "SLA Deadline", "SLA เกินกำหนด", "วันที่รับเรื่อง", "วันที่ปิด"
        };
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(1, c + 1).Value = headers[c];
            ws.Cell(1, c + 1).Style.Font.Bold = true;
            ws.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1A4A9A");
            ws.Cell(1, c + 1).Style.Font.FontColor = XLColor.White;
        }

        for (int row = 0; row < complaints.Count; row++)
        {
            var r = row + 2;
            var comp = complaints[row];
            ws.Cell(r, 1).Value  = comp.ReferenceNumber;
            ws.Cell(r, 2).Value  = comp.Category?.Name ?? "";
            ws.Cell(r, 3).Value  = comp.Status;
            ws.Cell(r, 4).Value  = comp.Priority;
            ws.Cell(r, 5).Value  = comp.ReporterName;
            ws.Cell(r, 6).Value  = comp.ReporterPhone;
            ws.Cell(r, 7).Value  = comp.AssignedTo?.FullName ?? "";
            ws.Cell(r, 8).Value  = comp.SlaDeadline.HasValue
                ? comp.SlaDeadline.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "";
            ws.Cell(r, 9).Value  = comp.SlaBreached ? "ใช่" : "ไม่";
            ws.Cell(r, 10).Value = comp.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            ws.Cell(r, 11).Value = comp.ClosedAt.HasValue
                ? comp.ClosedAt.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"complaints_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    private async Task LoadStatsAsync()
    {
        TotalAll      = await db.Complaints.CountAsync();
        TotalClosed   = await db.Complaints.CountAsync(c => c.Status == "Closed");
        TotalOpen     = await db.Complaints.CountAsync(c => c.Status != "Closed" && c.Status != "Rejected");
        TotalBreached = await db.Complaints.CountAsync(c => c.SlaBreached && c.Status != "Closed" && c.Status != "Rejected");

        // By category
        var catRaw = await db.Complaints
            .Join(db.ComplaintCategories, c => c.CategoryId, cat => cat.Id, (c, cat) => cat.Name)
            .GroupBy(name => name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();
        ByCategorySeries = catRaw
            .Select(x => new CategoryStat(x.Name ?? "ไม่ระบุ", x.Count))
            .ToList();

        // By status
        var statusRaw = await db.Complaints
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        ByStatusSeries = statusRaw
            .Select(x => new StatusStat(x.Status, x.Count))
            .ToList();

        // Monthly (last 12 months)
        var cutoff = DateTime.UtcNow.AddMonths(-11);
        var monthly = await db.Complaints
            .Where(c => c.CreatedAt >= cutoff)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        for (int m = 11; m >= 0; m--)
        {
            var d = DateTime.UtcNow.AddMonths(-m);
            MonthlyLabels.Add(d.ToString("MM/yy"));
            var found = monthly.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
            MonthlyData.Add(found?.Count ?? 0);
        }

        // Top staff (by closed cases, this month)
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var staffStats = await db.Complaints
            .Where(c => c.Status == "Closed" && c.ClosedAt >= monthStart && c.AssignedToId != null)
            .GroupBy(c => c.AssignedToId!.Value)
            .Select(g => new { StaffId = g.Key, Closed = g.Count() })
            .OrderByDescending(x => x.Closed)
            .Take(8)
            .ToListAsync();

        var staffIds = staffStats.Select(x => x.StaffId).ToList();
        var staffNames = staffIds.Any()
            ? await db.StaffUsers.Where(u => staffIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName)
            : new Dictionary<int, string>();

        TopStaff = staffStats
            .Select(x => new StaffStat(x.StaffId, staffNames.GetValueOrDefault(x.StaffId, "—"), x.Closed))
            .ToList();
    }
}

public record CategoryStat(string Name, int Count);
public record StatusStat(string Status, int Count);
public record StaffStat(int StaffId, string FullName, int ClosedThisMonth);
