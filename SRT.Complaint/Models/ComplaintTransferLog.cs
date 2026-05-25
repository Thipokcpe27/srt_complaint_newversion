#nullable enable
namespace SRT.Complaint.Models;

public class ComplaintTransferLog
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public int? FromOfficerId { get; set; }
    public int ToOfficerId { get; set; }
    public string? Reason { get; set; }
    public DateTime TransferredAt { get; set; } = DateTime.UtcNow;
    public bool IsAutoAssign { get; set; }

    public Complaint Complaint { get; set; } = null!;
    public StaffUser? FromOfficer { get; set; }
    public StaffUser ToOfficer { get; set; } = null!;
}
