using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using SRT.Complaint.Services;
using SRT.Complaint.Services.Adapters;

namespace SRT.Complaint.Pages.Admin;

// lock ป้องกัน concurrent write ไปยังไฟล์ appsettings.json
file static class AppSettingsLock
{
    internal static readonly SemaphoreSlim Gate = new(1, 1);
}

[Authorize(Roles = "SuperAdmin")]
public class SettingsModel(
    ISystemSettingService settings,
    IConfiguration config,
    IWebHostEnvironment env,
    IMemoryCache cache,
    ILogger<SettingsModel> logger) : PageModel
{
    // ── Group: org ──
    [BindProperty] public string OrgName      { get; set; } = string.Empty;
    [BindProperty] public string OrgNameShort { get; set; } = string.Empty;
    [BindProperty] public string OrgAddress   { get; set; } = string.Empty;
    [BindProperty] public string OrgPhone     { get; set; } = string.Empty;
    [BindProperty] public string OrgEmail     { get; set; } = string.Empty;
    [BindProperty] public string OrgWebsite   { get; set; } = string.Empty;

    // ── Group: notify ──
    [BindProperty] public bool   EmailEnabled   { get; set; }
    [BindProperty] public string SmtpHost       { get; set; } = string.Empty;
    [BindProperty] public int    SmtpPort       { get; set; } = 587;
    [BindProperty] public string SmtpUser       { get; set; } = string.Empty;
    [BindProperty] public string SmtpPass       { get; set; } = string.Empty;
    [BindProperty] public string SmtpFromName   { get; set; } = string.Empty;
    [BindProperty] public string SmtpFromEmail  { get; set; } = string.Empty;
    [BindProperty] public bool   SmsEnabled     { get; set; }
    [BindProperty] public string SmsGatewayUrl  { get; set; } = string.Empty;
    [BindProperty] public string SmsApiKey      { get; set; } = string.Empty;
    [BindProperty] public string SmsSender      { get; set; } = string.Empty;

    // ── Group: traffy ──
    [BindProperty] public bool   TraffyEnabled       { get; set; }
    [BindProperty] public string TraffyApiUrl        { get; set; } = string.Empty;
    [BindProperty] public string TraffyUsername      { get; set; } = string.Empty;
    [BindProperty] public string TraffyPassword      { get; set; } = string.Empty;
    [BindProperty] public string TraffyOrgId         { get; set; } = string.Empty;
    [BindProperty] public string TraffyWebhookSecret { get; set; } = string.Empty;

    // ── Group: maintenance ──
    [BindProperty] public bool   MaintenanceEnabled  { get; set; }
    [BindProperty] public string MaintenanceMessage  { get; set; } = string.Empty;
    [BindProperty] public string MaintenanceExpectedBack { get; set; } = string.Empty;

    // ── Group: security ──
    [BindProperty] public int SubmitLimitPerHour         { get; set; } = 5;
    [BindProperty] public int LoginLimitPerWindow        { get; set; } = 10;
    [BindProperty] public int LoginWindowMinutes         { get; set; } = 15;
    [BindProperty] public int TrackVerifyLimitPerWindow  { get; set; } = 10;
    [BindProperty] public int TrackVerifyWindowMinutes   { get; set; } = 15;
    [BindProperty] public int SessionTimeoutMinutes      { get; set; } = 30;

    public string ActiveTab { get; set; } = "org";

    public async Task OnGetAsync(string? tab)
    {
        ActiveTab = tab ?? "org";
        await LoadOrgAsync();
        await LoadNotifyAsync();
        await LoadTraffyAsync();
        await LoadMaintenanceAsync();
        LoadSecurity();
    }

    // ── org ──
    public async Task<IActionResult> OnPostOrgAsync()
    {
        await settings.SaveGroupAsync("org", new()
        {
            ["org.name"]       = OrgName.Trim(),
            ["org.name_short"] = OrgNameShort.Trim(),
            ["org.address"]    = OrgAddress.Trim(),
            ["org.phone"]      = OrgPhone.Trim(),
            ["org.email"]      = OrgEmail.Trim(),
            ["org.website"]    = OrgWebsite.Trim(),
        }, UserId());
        TempData["Success"] = "บันทึกข้อมูลองค์กรเรียบร้อยแล้ว";
        return RedirectToPage(new { tab = "org" });
    }

    // ── notify ──
    public async Task<IActionResult> OnPostNotifyAsync()
    {
        var values = new Dictionary<string, string>
        {
            ["notify.email_enabled"]   = EmailEnabled ? "true" : "false",
            ["notify.smtp_host"]       = SmtpHost.Trim(),
            ["notify.smtp_port"]       = SmtpPort.ToString(),
            ["notify.smtp_user"]       = SmtpUser.Trim(),
            ["notify.smtp_from_name"]  = SmtpFromName.Trim(),
            ["notify.smtp_from_email"] = SmtpFromEmail.Trim(),
            ["notify.sms_enabled"]     = SmsEnabled ? "true" : "false",
            ["notify.sms_gateway_url"] = SmsGatewayUrl.Trim(),
            ["notify.sms_sender"]      = SmsSender.Trim(),
        };
        if (!string.IsNullOrEmpty(SmtpPass))  values["notify.smtp_pass"]    = SmtpPass;
        if (!string.IsNullOrEmpty(SmsApiKey)) values["notify.sms_api_key"]  = SmsApiKey;

        await settings.SaveGroupAsync("notify", values, UserId());
        TempData["Success"] = "บันทึกการตั้งค่าแจ้งเตือนเรียบร้อยแล้ว";
        return RedirectToPage(new { tab = "notify" });
    }

    // ── traffy ──
    public async Task<IActionResult> OnPostTraffyAsync()
    {
        var values = new Dictionary<string, string>
        {
            ["traffy.enabled"] = TraffyEnabled ? "true" : "false",
            ["traffy.api_url"] = TraffyApiUrl.Trim(),
            ["traffy.username"]= TraffyUsername.Trim(),
            ["traffy.org_id"]  = TraffyOrgId.Trim(),
        };
        if (!string.IsNullOrEmpty(TraffyPassword))      values["traffy.password"]       = TraffyPassword;
        if (!string.IsNullOrEmpty(TraffyWebhookSecret)) values["traffy.webhook_secret"] = TraffyWebhookSecret;

        await settings.SaveGroupAsync("traffy", values, UserId());

        // ล้าง cache ทำให้ adapter โหลด settings ใหม่ทันที
        TraffyFonduAdapter.InvalidateCache(cache);

        TempData["Success"] = "บันทึกการตั้งค่า Traffy Fondue เรียบร้อยแล้ว";
        return RedirectToPage(new { tab = "traffy" });
    }

    // ── maintenance ──
    public async Task<IActionResult> OnPostMaintenanceAsync()
    {
        await settings.SaveGroupAsync("maintenance", new()
        {
            ["maintenance.enabled"]       = MaintenanceEnabled ? "true" : "false",
            ["maintenance.message"]       = MaintenanceMessage.Trim(),
            ["maintenance.expected_back"] = MaintenanceExpectedBack.Trim(),
        }, UserId());

        // ล้าง cache ทันทีเพื่อให้มีผลเร็ว
        cache.Remove("sys:maintenance.enabled");

        TempData["Success"] = MaintenanceEnabled
            ? "⚠️ เปิด Maintenance Mode แล้ว — ประชาชนจะเห็นหน้าปิดปรับปรุง"
            : "✅ ปิด Maintenance Mode แล้ว — ระบบกลับมาให้บริการปกติ";
        return RedirectToPage(new { tab = "maintenance" });
    }

    // ── security ──
    public async Task<IActionResult> OnPostSecurityAsync()
    {
        if (!await TryWriteAppSettingsAsync(new Dictionary<string, object>
        {
            ["Security:SubmitLimitPerHour"]        = SubmitLimitPerHour,
            ["Security:LoginLimitPerWindow"]        = LoginLimitPerWindow,
            ["Security:LoginWindowMinutes"]         = LoginWindowMinutes,
            ["Security:TrackVerifyLimitPerWindow"]  = TrackVerifyLimitPerWindow,
            ["Security:TrackVerifyWindowMinutes"]   = TrackVerifyWindowMinutes,
            ["Security:SessionTimeoutMinutes"]      = SessionTimeoutMinutes,
        }))
        {
            TempData["Warning"] = "บันทึกสำเร็จ แต่ไม่สามารถเขียนไฟล์ appsettings.json ได้ — กรุณาตั้งค่า Environment Variables แทน";
        }
        else
        {
            TempData["Success"] = "บันทึกการตั้งค่าความปลอดภัยเรียบร้อยแล้ว — มีผลหลังรีสตาร์ทแอปพลิเคชัน";
        }
        return RedirectToPage(new { tab = "security" });
    }

    // ── helpers ──
    private async Task LoadOrgAsync()
    {
        var org = await settings.GetGroupAsync("org");
        OrgName      = org.GetValueOrDefault("org.name",       "การรถไฟแห่งประเทศไทย");
        OrgNameShort = org.GetValueOrDefault("org.name_short", "รฟท.");
        OrgAddress   = org.GetValueOrDefault("org.address",    "");
        OrgPhone     = org.GetValueOrDefault("org.phone",      "1690");
        OrgEmail     = org.GetValueOrDefault("org.email",      "");
        OrgWebsite   = org.GetValueOrDefault("org.website",    "");
    }

    private async Task LoadNotifyAsync()
    {
        var n = await settings.GetGroupAsync("notify");
        EmailEnabled  = n.GetValueOrDefault("notify.email_enabled",  "true") == "true";
        SmtpHost      = n.GetValueOrDefault("notify.smtp_host",      "");
        SmtpPort      = int.TryParse(n.GetValueOrDefault("notify.smtp_port", "587"), out var p) ? p : 587;
        SmtpUser      = n.GetValueOrDefault("notify.smtp_user",      "");
        SmtpPass      = "";
        SmtpFromName  = n.GetValueOrDefault("notify.smtp_from_name", "ระบบรับเรื่องร้องเรียน รฟท.");
        SmtpFromEmail = n.GetValueOrDefault("notify.smtp_from_email","");
        SmsEnabled    = n.GetValueOrDefault("notify.sms_enabled",    "true") == "true";
        SmsGatewayUrl = n.GetValueOrDefault("notify.sms_gateway_url","");
        SmsApiKey     = "";
        SmsSender     = n.GetValueOrDefault("notify.sms_sender",     "SRT");
    }

    private async Task LoadTraffyAsync()
    {
        var t = await settings.GetGroupAsync("traffy");
        TraffyEnabled       = t.GetValueOrDefault("traffy.enabled",  "false") == "true";
        TraffyApiUrl        = t.GetValueOrDefault("traffy.api_url",  "https://publicapi.traffy.in.th/exchange-api");
        TraffyUsername      = t.GetValueOrDefault("traffy.username", "");
        TraffyPassword      = "";
        TraffyOrgId         = t.GetValueOrDefault("traffy.org_id",  "");
        TraffyWebhookSecret = "";
    }

    private async Task LoadMaintenanceAsync()
    {
        var m = await settings.GetGroupAsync("maintenance");
        MaintenanceEnabled      = m.GetValueOrDefault("maintenance.enabled",       "false") == "true";
        MaintenanceMessage      = m.GetValueOrDefault("maintenance.message",       "ระบบอยู่ระหว่างปรับปรุง กรุณากลับมาใหม่ในภายหลัง");
        MaintenanceExpectedBack = m.GetValueOrDefault("maintenance.expected_back", "");
    }

    private void LoadSecurity()
    {
        var sec = config.GetSection("Security");
        SubmitLimitPerHour        = sec.GetValue("SubmitLimitPerHour",        5);
        LoginLimitPerWindow       = sec.GetValue("LoginLimitPerWindow",       10);
        LoginWindowMinutes        = sec.GetValue("LoginWindowMinutes",        15);
        TrackVerifyLimitPerWindow = sec.GetValue("TrackVerifyLimitPerWindow", 10);
        TrackVerifyWindowMinutes  = sec.GetValue("TrackVerifyWindowMinutes",  15);
        SessionTimeoutMinutes     = sec.GetValue("SessionTimeoutMinutes",     30);
    }

    private async Task<bool> TryWriteAppSettingsAsync(Dictionary<string, object> updates)
    {
        await AppSettingsLock.Gate.WaitAsync();
        try
        {
            var path = Path.Combine(env.ContentRootPath, "appsettings.json");
            var json = await System.IO.File.ReadAllTextAsync(path);
            var doc  = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
            var root = doc.ToDictionary(k => k.Key, v => (object)v.Value);

            foreach (var (key, value) in updates)
                SetNestedValue(root, key.Split(':'), value);

            var opts    = new JsonSerializerOptions { WriteIndented = true };
            var newJson = JsonSerializer.Serialize(root, opts);

            // เขียนแบบ atomic: temp file → File.Replace
            var tmp = path + ".tmp";
            await System.IO.File.WriteAllTextAsync(tmp, newJson);
            System.IO.File.Replace(tmp, path, null);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ไม่สามารถเขียน appsettings.json ได้ — ตรวจสอบ permission ของ app pool");
            return false;
        }
        finally
        {
            AppSettingsLock.Gate.Release();
        }
    }

    private static void SetNestedValue(Dictionary<string, object> dict, string[] keys, object value)
    {
        if (keys.Length == 1)
        {
            dict[keys[0]] = value;
            return;
        }
        if (!dict.TryGetValue(keys[0], out var existing) || existing is not Dictionary<string, object> nested)
        {
            nested = [];
            if (existing is JsonElement je && je.ValueKind == JsonValueKind.Object)
                nested = je.EnumerateObject().ToDictionary(p => p.Name, p => (object)p.Value);
            dict[keys[0]] = nested;
        }
        SetNestedValue(nested, keys[1..], value);
    }

    private int UserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
