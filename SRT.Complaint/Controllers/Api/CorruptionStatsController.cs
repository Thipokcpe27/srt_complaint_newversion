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
[Route("api/stats/corruption")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class CorruptionStatsController(
    CorruptionDbContext corruptionDb,
    IApiRequestLogService logService,
    ILogger<CorruptionStatsController> logger) : ControllerBase
{
    // ─── GET /api/stats/corruption ────────────────────────────────────────────
    [HttpGet]
    [RequireScope("corruption:stats")]
    public async Task<IActionResult> GetCorruptionStats(CancellationToken ct)
    {
        int statusCode = 200;
        var sw = Stopwatch.StartNew();
        try
        {
            var now   = DateTime.UtcNow;
            var today = now.Date;

            var total      = await corruptionDb.Reports.CountAsync(ct);
            var pending    = await corruptionDb.Reports.CountAsync(r => r.Status == "Pending", ct);
            var inProgress = await corruptionDb.Reports.CountAsync(r => r.Status == "InProgress", ct);
            var underReview= await corruptionDb.Reports.CountAsync(r => r.Status == "UnderReview", ct);
            var closed     = await corruptionDb.Reports.CountAsync(r => r.Status == "Closed", ct);
            var rejected   = await corruptionDb.Reports.CountAsync(r => r.Status == "Rejected", ct);
            var breached   = await corruptionDb.Reports.CountAsync(r => r.SlaBreached && r.Status != "Closed" && r.Status != "Rejected", ct);
            var todayNew   = await corruptionDb.Reports.CountAsync(r => r.CreatedAt >= today, ct);

            var bySubjectType = await corruptionDb.Reports
                .GroupBy(r => r.SubjectType)
                .Select(g => new { subjectType = g.Key, count = g.Count() })
                .ToListAsync(ct);

            return Ok(new
            {
                asOf = now,
                reports = new
                {
                    total, pending, inProgress, underReview, closed, rejected,
                    slaBreached = breached,
                    todayNew
                },
                bySubjectType
            });
        }
        catch (Exception ex)
        {
            statusCode = 500;
            logger.LogError(ex, "Error getting corruption stats");
            return StatusCode(500, new { error = "Internal server error" });
        }
        finally
        {
            sw.Stop();
            await LogRequestAsync("GET", "/api/stats/corruption", null, statusCode, (int)sw.ElapsedMilliseconds);
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
