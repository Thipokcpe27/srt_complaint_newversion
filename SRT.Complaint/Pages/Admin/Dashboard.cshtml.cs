#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class DashboardModel(AppDbContext db, CorruptionDbContext corruptionDb) : PageModel
{
    public int TotalStaff       { get; private set; }
    public int ActiveStaff      { get; private set; }
    public int TotalComplaints  { get; private set; }
    public int OpenComplaints   { get; private set; }
    public int SlaBreachedOpen  { get; private set; }
    public int TotalCorruption  { get; private set; }
    public int OpenCorruption   { get; private set; }
    public int ActiveApiKeys    { get; private set; }
    public long TotalAuditLogs  { get; private set; }
    public IReadOnlyList<AuditLog> RecentLogs { get; private set; } = [];
    public Dictionary<int, string> ActorNames { get; private set; } = new();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "ภาพรวมระบบ";

        TotalStaff      = await db.StaffUsers.CountAsync();
        ActiveStaff     = await db.StaffUsers.CountAsync(u => u.IsActive);
        TotalComplaints = await db.Complaints.CountAsync();
        OpenComplaints  = await db.Complaints.CountAsync(c => c.Status != "Closed" && c.Status != "Rejected");
        SlaBreachedOpen = await db.Complaints.CountAsync(c => c.SlaBreached && c.Status != "Closed" && c.Status != "Rejected");
        TotalCorruption = await corruptionDb.Reports.CountAsync();
        OpenCorruption  = await corruptionDb.Reports.CountAsync(r => r.Status != "Closed" && r.Status != "Rejected");
        ActiveApiKeys   = await db.ApiKeys.CountAsync(k => k.IsActive);
        TotalAuditLogs  = await db.AuditLogs.LongCountAsync();

        RecentLogs = await db.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(15)
            .ToListAsync();

        var actorIds = RecentLogs
            .Where(l => l.ActorId.HasValue)
            .Select(l => l.ActorId!.Value)
            .Distinct()
            .ToList();

        ActorNames = actorIds.Count > 0
            ? await db.StaffUsers
                .Where(u => actorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName)
            : new Dictionary<int, string>();
    }
}
