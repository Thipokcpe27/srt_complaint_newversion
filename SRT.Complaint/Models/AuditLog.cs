#nullable enable
namespace SRT.Complaint.Models;

public class AuditLog
{
    public long Id { get; set; }
    public int? ActorId { get; set; }
    public string? ActorCode { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Detail { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StaffUser? Actor { get; set; }
}
