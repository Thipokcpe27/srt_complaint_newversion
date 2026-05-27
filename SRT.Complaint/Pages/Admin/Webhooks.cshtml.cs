#nullable enable
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class WebhooksModel(IWebhookService webhookService, IAuditService auditService) : PageModel
{
    public IReadOnlyList<Webhook> Webhooks { get; private set; } = [];

    [BindProperty] public int DeleteId { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Webhooks";
        Webhooks = await webhookService.ListAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        await webhookService.DeleteAsync(DeleteId);
        await auditService.LogAsync("DeleteWebhook", GetActorId(), GetActorCode(),
            "Webhook", DeleteId.ToString(), null, GetIp());
        TempData["Success"] = "ลบ Webhook เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public static List<string> ParseEvents(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }

    private int    GetActorId()   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetActorCode() => User.FindFirstValue("EmployeeCode") ?? "";
    private string GetIp()        => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
