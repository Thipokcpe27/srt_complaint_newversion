using System.Text.Json;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class AuditService(AppDbContext db) : IAuditService
{
    public async Task LogAsync(string action, int? actorId, string? actorCode, string? entityType, string? entityId,
                               object? detail, string? ipAddress,
                               string? userAgent = null, string? outcome = null,
                               CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Action    = action,
            ActorId   = actorId,
            ActorCode = actorCode,
            EntityType = entityType,
            EntityId   = entityId,
            Detail     = detail is null ? null : JsonSerializer.Serialize(detail),
            IpAddress  = ipAddress,
            UserAgent  = userAgent is { Length: > 500 } ? userAgent[..500] : userAgent,
            Outcome    = outcome,
            CreatedAt  = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
