using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;

namespace SRT.Complaint.Services;

public class CorruptionStatsService(CorruptionDbContext db) : ICorruptionStatsService
{
    public async Task<CorruptionSummaryStats> GetSummaryAsync(CancellationToken ct = default)
    {
        var now   = DateTime.UtcNow;
        var today = now.Date;

        var total      = await db.Reports.CountAsync(ct);
        var pending    = await db.Reports.CountAsync(r => r.Status == "Pending", ct);
        var inProgress = await db.Reports.CountAsync(r => r.Status == "InProgress", ct);
        var underReview= await db.Reports.CountAsync(r => r.Status == "UnderReview", ct);
        var closed     = await db.Reports.CountAsync(r => r.Status == "Closed", ct);
        var rejected   = await db.Reports.CountAsync(r => r.Status == "Rejected", ct);
        var breached   = await db.Reports.CountAsync(r => r.SlaBreached && r.Status != "Closed" && r.Status != "Rejected", ct);
        var todayNew   = await db.Reports.CountAsync(r => r.CreatedAt >= today, ct);

        var bySubjectType = await db.Reports
            .GroupBy(r => r.SubjectType)
            .Select(g => new SubjectTypeStat(g.Key, g.Count()))
            .ToListAsync(ct);

        return new CorruptionSummaryStats(now,
            total, pending, inProgress, underReview, closed, rejected,
            breached, todayNew, bySubjectType);
    }
}
