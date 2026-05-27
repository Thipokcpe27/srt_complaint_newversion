#nullable enable
using Microsoft.AspNetCore.Mvc;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Controllers.Api;

public abstract class ApiBaseController(
    IApiRequestLogService logService,
    ILogger logger) : ControllerBase
{
    protected async Task LogRequestAsync(string method, string endpoint, string? query, int status, int ms)
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
