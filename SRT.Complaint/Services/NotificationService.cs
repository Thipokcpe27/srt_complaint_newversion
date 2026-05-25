using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;

namespace SRT.Complaint.Services;

public class NotificationService(AppDbContext db, IConfiguration config, ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendAsync(string eventKey, string? toPhone, string? toEmail, Dictionary<string, string> placeholders, CancellationToken ct = default)
    {
        var template = await db.NotificationTemplates.FirstOrDefaultAsync(t => t.EventKey == eventKey, ct);
        if (template is null) return;

        if (template.IsSmsEnabled && !string.IsNullOrWhiteSpace(toPhone) && !string.IsNullOrWhiteSpace(template.SmsBody))
            await SendSmsAsync(toPhone, ResolvePlaceholders(template.SmsBody, placeholders), ct);

        if (template.IsEmailEnabled && !string.IsNullOrWhiteSpace(toEmail) && !string.IsNullOrWhiteSpace(template.EmailBody))
            await SendEmailAsync(toEmail, ResolvePlaceholders(template.EmailSubject ?? eventKey, placeholders), ResolvePlaceholders(template.EmailBody, placeholders), ct);
    }

    private static string ResolvePlaceholders(string template, Dictionary<string, string> values)
    {
        foreach (var (k, v) in values)
            template = template.Replace($"{{{k}}}", v);
        return template;
    }

    private async Task SendSmsAsync(string phone, string body, CancellationToken ct)
    {
        try
        {
            var gatewayUrl = config["Notifications:SmsGatewayUrl"];
            var apiKey = config["Notifications:SmsApiKey"];
            if (string.IsNullOrEmpty(gatewayUrl)) return;

            using var http = new HttpClient();
            await http.PostAsJsonAsync(gatewayUrl, new { phone, message = body, apiKey }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SMS send failed to {Phone}", phone);
        }
    }

    private async Task SendEmailAsync(string toAddress, string subject, string body, CancellationToken ct)
    {
        try
        {
            var host = config["Notifications:SmtpHost"];
            if (string.IsNullOrEmpty(host)) return;

            var port = config.GetValue<int>("Notifications:SmtpPort", 587);
            var user = config["Notifications:SmtpUser"] ?? "";
            var pass = config["Notifications:SmtpPassword"] ?? "";

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(user));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(user, pass, ct);
            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Email send failed to {Email}", toAddress);
        }
    }
}
