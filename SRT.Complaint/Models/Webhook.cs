#nullable enable
namespace SRT.Complaint.Models;

public class Webhook
{
    public int Id { get; set; }
    public int ApiKeyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string SecretHash { get; set; } = string.Empty;
    public string Events { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastTriggeredAt { get; set; }
    public int? LastStatusCode { get; set; }

    public ApiKey ApiKey { get; set; } = null!;
    public ICollection<WebhookDeliveryLog> DeliveryLogs { get; set; } = new List<WebhookDeliveryLog>();
}
