namespace SRT.Complaint.Models;

public class ApiRequestLog
{
    public long Id { get; set; }
    public int ApiKeyId { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public string? IpAddress { get; set; }
    public int ResponseStatus { get; set; }
    public int? ResponseMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApiKey ApiKey { get; set; } = null!;
}
