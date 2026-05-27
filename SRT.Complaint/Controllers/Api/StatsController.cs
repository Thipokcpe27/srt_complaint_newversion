#nullable enable
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Filters;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Controllers.Api;

[ApiController]
[Route("api/stats")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class StatsController(
    AppDbContext db,
    IApiRequestLogService logService,
    ILogger<StatsController> logger) : ControllerBase
{
    // ─── GET /api/stats/summary ───────────────────────────────────────────────
    [HttpGet("summary")]
    [RequireScope("stats:summary")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        int statusCode = 200;
        var sw = Stopwatch.StartNew();
        try
        {
            var now   = DateTime.UtcNow;
            var today = now.Date;

            var totalTask     = db.Complaints.CountAsync(ct);
            var pendingTask   = db.Complaints.CountAsync(c => c.Status == "Pending", ct);
            var inProgressTask= db.Complaints.CountAsync(c => c.Status == "InProgress", ct);
            var resolvedTask  = db.Complaints.CountAsync(c => c.Status == "Resolved", ct);
            var closedTask    = db.Complaints.CountAsync(c => c.Status == "Closed", ct);
            var rejectedTask  = db.Complaints.CountAsync(c => c.Status == "Rejected", ct);
            var breachedTask  = db.Complaints.CountAsync(c => c.SlaBreached && c.Status != "Closed" && c.Status != "Rejected", ct);
            var todayTask     = db.Complaints.CountAsync(c => c.CreatedAt >= today, ct);

            await Task.WhenAll(totalTask, pendingTask, inProgressTask, resolvedTask, closedTask, rejectedTask, breachedTask, todayTask);

            return Ok(new
            {
                asOf = now,
                complaints = new
                {
                    total      = totalTask.Result,
                    pending    = pendingTask.Result,
                    inProgress = inProgressTask.Result,
                    resolved   = resolvedTask.Result,
                    closed     = closedTask.Result,
                    rejected   = rejectedTask.Result,
                    slaBreached = breachedTask.Result,
                    todayNew   = todayTask.Result
                }
            });
        }
        catch (Exception ex)
        {
            statusCode = 500;
            logger.LogError(ex, "Error getting stats summary");
            return StatusCode(500, new { error = "Internal server error" });
        }
        finally
        {
            sw.Stop();
            await LogRequestAsync("GET", "/api/stats/summary", null, statusCode, (int)sw.ElapsedMilliseconds);
        }
    }

    // ─── GET /api/stats/detailed ─────────────────────────────────────────────
    [HttpGet("detailed")]
    [RequireScope("stats:detailed")]
    public async Task<IActionResult> GetDetailedStats(CancellationToken ct)
    {
        int statusCode = 200;
        var sw = Stopwatch.StartNew();
        try
        {
            var now   = DateTime.UtcNow;
            var today = now.Date;

            var byCategory = await db.Complaints
                .GroupBy(c => c.Category.Name)
                .Select(g => new { category = g.Key, total = g.Count(), open = g.Count(c => c.Status != "Closed" && c.Status != "Rejected") })
                .ToListAsync(ct);

            var byPriority = await db.Complaints
                .GroupBy(c => c.Priority)
                .Select(g => new { priority = g.Key, count = g.Count() })
                .ToListAsync(ct);

            var byStatus = await db.Complaints
                .GroupBy(c => c.Status)
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToListAsync(ct);

            var closedComplaints = await db.Complaints
                .Where(c => c.ClosedAt.HasValue)
                .Select(c => new { c.CreatedAt, ClosedAt = c.ClosedAt!.Value })
                .ToListAsync(ct);

            var avgResolutionHours = closedComplaints.Any()
                ? closedComplaints.Average(c => (c.ClosedAt - c.CreatedAt).TotalHours)
                : 0.0;

            return Ok(new
            {
                asOf = now,
                byCategory,
                byPriority,
                byStatus,
                averageResolutionHours = Math.Round(avgResolutionHours, 1)
            });
        }
        catch (Exception ex)
        {
            statusCode = 500;
            logger.LogError(ex, "Error getting detailed stats");
            return StatusCode(500, new { error = "Internal server error" });
        }
        finally
        {
            sw.Stop();
            await LogRequestAsync("GET", "/api/stats/detailed", null, statusCode, (int)sw.ElapsedMilliseconds);
        }
    }

    private async Task LogRequestAsync(string method, string endpoint, string? query, int status, int ms)
    {
        try
        {
            var apiKey = HttpContext.Items["ApiKey"] as ApiKey;
            if (apiKey != null)
                await logService.LogAsync(apiKey.Id, method, endpoint, query,
                    HttpContext.Connection.RemoteIpAddress?.ToString(), status, ms);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to log API request");
        }
    }
}
