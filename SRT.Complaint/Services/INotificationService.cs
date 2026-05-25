namespace SRT.Complaint.Services;

public interface INotificationService
{
    Task SendAsync(string eventKey, string? toPhone, string? toEmail, Dictionary<string, string> placeholders, CancellationToken ct = default);
}
