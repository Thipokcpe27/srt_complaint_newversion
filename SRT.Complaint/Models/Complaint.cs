#nullable enable
namespace SRT.Complaint.Models;

public class Complaint
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public string ReporterName { get; set; } = string.Empty;
    public string ReporterPhone { get; set; } = string.Empty;
    public string? ReporterEmail { get; set; }

    public int CategoryId { get; set; }
    public string? SubjectStation { get; set; }
    public DateOnly? IncidentDate { get; set; }
    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Pending";
    public int? AssignedToId { get; set; }
    public DateTime? AssignedAt { get; set; }

    public DateTime? SlaDeadline { get; set; }
    public bool SlaBreached { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public byte? SatisfactionScore { get; set; }
    public string? SatisfactionNote { get; set; }

    public ComplaintCategory Category { get; set; } = null!;
    public StaffUser? AssignedTo { get; set; }
    public ICollection<ComplaintAttachment> Attachments { get; set; } = new List<ComplaintAttachment>();
    public ICollection<ComplaintNote> Notes { get; set; } = new List<ComplaintNote>();
    public ICollection<ComplaintTransferLog> TransferLogs { get; set; } = new List<ComplaintTransferLog>();
}
