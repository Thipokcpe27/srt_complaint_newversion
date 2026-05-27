#nullable enable
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SRT.Complaint.Filters;
using SRT.Complaint.Services;

namespace SRT.Complaint.Controllers.Api;

[ApiController]
[Route("api/stats/corruption")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class CorruptionStatsController(
    ICorruptionStatsService corruptionStatsService,
    IApiRequestLogService logService,
    ILogger<CorruptionStatsController> logger) : ApiBaseController(logService, logger)
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
            var s = await corruptionStatsService.GetSummaryAsync(ct);
            return Ok(new
            {
                asOf = s.AsOf,
                reports = new
                {
                    total       = s.Total,
                    pending     = s.Pending,
                    inProgress  = s.InProgress,
                    underReview = s.UnderReview,
                    closed      = s.Closed,
                    rejected    = s.Rejected,
                    slaBreached = s.SlaBreached,
                    todayNew    = s.TodayNew
                },
                bySubjectType = s.BySubjectType
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
}
