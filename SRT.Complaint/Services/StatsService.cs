using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;

namespace SRT.Complaint.Services;

public class StatsService(AppDbContext db) : IStatsService
{
    public async Task<ComplaintSummaryStats> GetSummaryAsync(CancellationToken ct = default)
    {
        var now   = DateTime.UtcNow;
        var today = now.Date;

        var totalTask      = db.Complaints.CountAsync(ct);
        var pendingTask    = db.Complaints.CountAsync(c => c.Status == "Pending", ct);
        var inProgressTask = db.Complaints.CountAsync(c => c.Status == "InProgress", ct);
        var resolvedTask   = db.Complaints.CountAsync(c => c.Status == "Resolved", ct);
        var closedTask     = db.Complaints.CountAsync(c => c.Status == "Closed", ct);
        var rejectedTask   = db.Complaints.CountAsync(c => c.Status == "Rejected", ct);
        var breachedTask   = db.Complaints.CountAsync(c => c.SlaBreached && c.Status != "Closed" && c.Status != "Rejected", ct);
        var todayTask      = db.Complaints.CountAsync(c => c.CreatedAt >= today, ct);

        await Task.WhenAll(totalTask, pendingTask, inProgressTask, resolvedTask,
                           closedTask, rejectedTask, breachedTask, todayTask);

        return new ComplaintSummaryStats(now,
            totalTask.Result, pendingTask.Result, inProgressTask.Result,
            resolvedTask.Result, closedTask.Result, rejectedTask.Result,
            breachedTask.Result, todayTask.Result);
    }

    public async Task<ComplaintDetailedStats> GetDetailedAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var byCategory = await db.Complaints
            .GroupBy(c => c.Category.Name)
            .Select(g => new CategoryStat(
                g.Key,
                g.Count(),
                g.Count(c => c.Status != "Closed" && c.Status != "Rejected")))
            .ToListAsync(ct);

        var byPriority = await db.Complaints
            .GroupBy(c => c.Priority)
            .Select(g => new PriorityStat(g.Key, g.Count()))
            .ToListAsync(ct);

        var byStatus = await db.Complaints
            .GroupBy(c => c.Status)
            .Select(g => new StatusStat(g.Key, g.Count()))
            .ToListAsync(ct);

        var closedComplaints = await db.Complaints
            .Where(c => c.ClosedAt.HasValue)
            .Select(c => new { c.CreatedAt, ClosedAt = c.ClosedAt!.Value })
            .ToListAsync(ct);

        var avg = closedComplaints.Count > 0
            ? Math.Round(closedComplaints.Average(c => (c.ClosedAt - c.CreatedAt).TotalHours), 1)
            : 0.0;

        return new ComplaintDetailedStats(now, byCategory, byPriority, byStatus, avg);
    }
}
