namespace SRT.Complaint.Models;

public class ApiKeyScope
{
    public int Id { get; set; }
    public int ApiKeyId { get; set; }
    public string Scope { get; set; } = string.Empty;

    public ApiKey ApiKey { get; set; } = null!;
}
