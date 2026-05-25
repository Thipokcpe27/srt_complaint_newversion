#nullable enable
namespace SRT.Complaint.Models;

public class SlaConfig
{
    public int Id { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string LabelTh { get; set; } = string.Empty;
    public int ResolutionHours { get; set; }
    public int AutoAssignAfterHours { get; set; }
    public int WarningThresholdPercent { get; set; } = 80;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }

    public StaffUser? UpdatedBy { get; set; }
}
