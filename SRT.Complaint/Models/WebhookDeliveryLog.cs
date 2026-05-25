#nullable enable
namespace SRT.Complaint.Models;

public class WebhookDeliveryLog
{
    public long Id { get; set; }
    public int WebhookId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public byte AttemptCount { get; set; } = 1;
    public int? ResponseStatus { get; set; }
    public string? ResponseBody { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }

    public Webhook Webhook { get; set; } = null!;
}
