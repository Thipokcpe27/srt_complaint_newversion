namespace SRT.Complaint.Services;

public interface IAuditService
{
    Task LogAsync(string action, int? actorId, string? actorCode, string? entityType, string? entityId,
                  object? detail, string? ipAddress,
                  string? userAgent = null, string? outcome = null,
                  CancellationToken ct = default);
}
