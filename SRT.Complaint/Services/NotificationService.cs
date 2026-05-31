using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;

namespace SRT.Complaint.Services;

public class NotificationService(
    AppDbContext db,
    IConfiguration config,
    ISystemSettingService sysSettings,
    IHttpClientFactory httpFactory,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendAsync(string eventKey, string? toPhone, string? toEmail,
        Dictionary<string, string> placeholders, CancellationToken ct = default)
    {
        var template = await db.NotificationTemplates.FirstOrDefaultAsync(t => t.EventKey == eventKey, ct);
        if (template is null) return;

        var notify = await sysSettings.GetGroupAsync("notify");
        bool emailEnabled = notify.GetValueOrDefault("notify.email_enabled", "true") == "true";
        bool smsEnabled   = notify.GetValueOrDefault("notify.sms_enabled",   "true") == "true";

        if (smsEnabled && template.IsSmsEnabled && !string.IsNullOrWhiteSpace(toPhone) && !string.IsNullOrWhiteSpace(template.SmsBody))
            await SendSmsAsync(toPhone, ResolvePlaceholders(template.SmsBody, placeholders), notify, ct);

        if (emailEnabled && template.IsEmailEnabled && !string.IsNullOrWhiteSpace(toEmail) && !string.IsNullOrWhiteSpace(template.EmailBody))
            await SendEmailAsync(toEmail,
                ResolvePlaceholders(template.EmailSubject ?? eventKey, placeholders),
                ResolvePlaceholders(template.EmailBody, placeholders),
                notify, ct);
    }

    private static string ResolvePlaceholders(string template, Dictionary<string, string> values)
    {
        foreach (var (k, v) in values)
            template = template.Replace($"{{{k}}}", v);
        return template;
    }

    // คืนค่า DB ถ้าไม่ว่าง, ไม่อย่างนั้น fallback ไป config
    // GetValueOrDefault คืน "" เมื่อ key มีอยู่แต่ว่าง ซึ่ง ?? จะไม่ fallback
    private static string? DbOrConfig(Dictionary<string, string> n, string dbKey, string? configVal)
    {
        var v = n.GetValueOrDefault(dbKey);
        return string.IsNullOrEmpty(v) ? configVal : v;
    }

    private async Task SendSmsAsync(string phone, string body, Dictionary<string, string> n, CancellationToken ct)
    {
        try
        {
            var gatewayUrl = DbOrConfig(n, "notify.sms_gateway_url", config["Notifications:SmsGatewayUrl"]);
            var apiKey     = DbOrConfig(n, "notify.sms_api_key",     config["Notifications:SmsApiKey"]);

            if (string.IsNullOrEmpty(gatewayUrl)) return;

            using var http = httpFactory.CreateClient("Sms");
            await http.PostAsJsonAsync(gatewayUrl, new { phone, message = body, apiKey }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SMS send failed to {Phone}", phone);
        }
    }

    private async Task SendEmailAsync(string toAddress, string subject, string body,
        Dictionary<string, string> n, CancellationToken ct)
    {
        try
        {
            var host = DbOrConfig(n, "notify.smtp_host", config["Notifications:SmtpHost"]);
            if (string.IsNullOrEmpty(host)) return;

            var portStr  = DbOrConfig(n, "notify.smtp_port", null);
            var port     = int.TryParse(portStr, out var p) ? p
                         : config.GetValue<int>("Notifications:SmtpPort", 587);
            var user     = DbOrConfig(n, "notify.smtp_user",       config["Notifications:SmtpUser"])     ?? "";
            var pass     = DbOrConfig(n, "notify.smtp_pass",       config["Notifications:SmtpPassword"]) ?? "";
            var fromName = DbOrConfig(n, "notify.smtp_from_name",  config["Notifications:SmtpFromName"]) ?? "ระบบรับเรื่องร้องเรียน รฟท.";
            var fromAddr = DbOrConfig(n, "notify.smtp_from_email", null) ?? user;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddr));
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
