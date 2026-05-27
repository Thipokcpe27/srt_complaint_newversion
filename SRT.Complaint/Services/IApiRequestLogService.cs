namespace SRT.Complaint.Services;

public interface IApiRequestLogService
{
    Task LogAsync(int apiKeyId, string httpMethod, string endpoint,
        string? queryString, string? ipAddress, int statusCode, int? durationMs,
        CancellationToken ct = default);
}
