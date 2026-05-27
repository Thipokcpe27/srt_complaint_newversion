using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class ApiRequestLogService(AppDbContext db) : IApiRequestLogService
{
    public async Task LogAsync(int apiKeyId, string httpMethod, string endpoint,
        string? queryString, string? ipAddress, int statusCode, int? durationMs,
        CancellationToken ct = default)
    {
        db.ApiRequestLogs.Add(new ApiRequestLog
        {
            ApiKeyId       = apiKeyId,
            HttpMethod     = httpMethod,
            Endpoint       = endpoint,
            QueryString    = queryString,
            IpAddress      = ipAddress,
            ResponseStatus = statusCode,
            ResponseMs     = durationMs,
            CreatedAt      = DateTime.UtcNow
        });

        var key = await db.ApiKeys.FindAsync([apiKeyId], ct);
        if (key != null) key.LastUsedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
