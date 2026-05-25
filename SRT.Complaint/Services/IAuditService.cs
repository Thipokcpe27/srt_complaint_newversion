namespace SRT.Complaint.Services;

public interface IAuditService
{
    Task LogAsync(string action, int? actorId, string? actorCode, string? entityType, string? entityId, object? detail, string? ipAddress, CancellationToken ct = default);
}
