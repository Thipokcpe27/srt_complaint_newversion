#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class ApiKeysModel(IApiKeyService apiKeyService, IAuditService auditService) : PageModel
{
    public IReadOnlyList<ApiKey> Keys { get; private set; } = [];

    // Create form
    [BindProperty] public string?       NewName           { get; set; }
    [BindProperty] public string        NewKeyType        { get; set; } = "External";
    [BindProperty] public List<string>  NewScopes         { get; set; } = new();
    [BindProperty] public int           NewRateLimit      { get; set; } = 60;
    [BindProperty] public string?       NewAllowedIps     { get; set; }
    [BindProperty] public DateTime?     NewExpiresAt      { get; set; }

    // Revoke form
    [BindProperty] public int     RevokeId     { get; set; }
    [BindProperty] public string? RevokeReason { get; set; }

    public static readonly string[] AvailableScopes =
    [
        "complaints:read",
        "complaints:write",
        "complaints:edoc",
        "complaints:status",
        "complaints:update",
        "corruption:read",
        "corruption:stats",
        "admin:read",
        "stats:summary",
        "stats:detailed",
        "webhooks:manage"
    ];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "API Keys";
        Keys = await apiKeyService.ListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            TempData["Error"] = "กรุณากรอกชื่อ API Key";
            return RedirectToPage();
        }
        if (!NewScopes.Any())
        {
            TempData["Error"] = "กรุณาเลือก Scope อย่างน้อย 1 รายการ";
            return RedirectToPage();
        }

        var request = new CreateApiKeyRequest(
            Name:           NewName.Trim(),
            KeyType:        NewKeyType,
            Scopes:         NewScopes,
            RateLimitPerMin: NewRateLimit > 0 ? NewRateLimit : 60,
            AllowedIps:     string.IsNullOrWhiteSpace(NewAllowedIps) ? null : NewAllowedIps.Trim(),
            ExpiresAt:      NewExpiresAt.HasValue ? NewExpiresAt.Value.ToUniversalTime() : null,
            CreatedById:    GetActorId()
        );

        var (key, rawKey) = await apiKeyService.CreateAsync(request);
        await auditService.LogAsync("CreateApiKey", GetActorId(), GetActorCode(),
            "ApiKey", key.Id.ToString(), new { key.Name, key.KeyType, Scopes = NewScopes }, GetIp());

        TempData["NewRawKey"]  = rawKey;
        TempData["NewKeyName"] = key.Name;
        TempData["Success"]    = $"สร้าง API Key \"{key.Name}\" เรียบร้อยแล้ว — คัดลอก Key ด้านล่างก่อนออกจากหน้านี้";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRevokeAsync()
    {
        if (string.IsNullOrWhiteSpace(RevokeReason))
        {
            TempData["Error"] = "กรุณาระบุเหตุผลในการยกเลิก";
            return RedirectToPage();
        }
        try
        {
            await apiKeyService.RevokeAsync(RevokeId, GetActorId(), RevokeReason.Trim());
            await auditService.LogAsync("RevokeApiKey", GetActorId(), GetActorCode(),
                "ApiKey", RevokeId.ToString(), new { Reason = RevokeReason.Trim() }, GetIp());
            TempData["Success"] = "ยกเลิก API Key เรียบร้อยแล้ว";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }

    private int    GetActorId()   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetActorCode() => User.FindFirstValue("EmployeeCode") ?? "";
    private string GetIp()        => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
