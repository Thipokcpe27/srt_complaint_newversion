#nullable enable
namespace SRT.Complaint.Services;

public interface ITurnstileService
{
    Task<bool> VerifyAsync(string token, string? remoteIp = null);
}
