#nullable enable
namespace SRT.Complaint.Models;

public class NotificationTemplate
{
    public int Id { get; set; }
    public string EventKey { get; set; } = string.Empty;
    public string LabelTh { get; set; } = string.Empty;
    public string? EmailSubject { get; set; }
    public string? EmailBody { get; set; }
    public string? SmsBody { get; set; }
    public bool IsEmailEnabled { get; set; } = true;
    public bool IsSmsEnabled { get; set; } = true;
}
