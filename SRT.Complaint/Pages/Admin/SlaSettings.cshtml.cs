#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class SlaSettingsModel(AppDbContext db, IAuditService auditService) : PageModel
{
    [BindProperty] public List<SlaConfigInput> Configs { get; set; } = new();

    public IReadOnlyList<SlaConfig> SlaConfigs { get; private set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "SLA Settings";
        SlaConfigs = await db.SlaConfigs.OrderBy(s => s.Id).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!Configs.Any())
        {
            TempData["Error"] = "ไม่พบข้อมูลที่จะบันทึก";
            return RedirectToPage();
        }

        var configs = await db.SlaConfigs.ToListAsync();
        foreach (var input in Configs)
        {
            var config = configs.FirstOrDefault(c => c.Id == input.Id);
            if (config == null) continue;
            if (input.ResolutionHours <= 0 || input.AutoAssignAfterHours <= 0)
            {
                TempData["Error"] = $"ค่าชั่วโมงต้องมากกว่า 0 (Priority: {config.Priority})";
                return RedirectToPage();
            }
            config.ResolutionHours         = input.ResolutionHours;
            config.AutoAssignAfterHours    = input.AutoAssignAfterHours;
            config.WarningThresholdPercent = Math.Clamp(input.WarningThresholdPercent, 10, 99);
            config.UpdatedAt               = DateTime.UtcNow;
            config.UpdatedById             = GetActorId();
        }
        await db.SaveChangesAsync();
        await auditService.LogAsync("UpdateSlaSettings", GetActorId(), GetActorCode(),
            "SlaConfig", null, null, GetIp());

        TempData["Success"] = "บันทึกการตั้งค่า SLA เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    private int    GetActorId()   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetActorCode() => User.FindFirstValue("EmployeeCode") ?? "";
    private string GetIp()        => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

public class SlaConfigInput
{
    public int Id                      { get; set; }
    public int ResolutionHours         { get; set; }
    public int AutoAssignAfterHours    { get; set; }
    public int WarningThresholdPercent { get; set; }
}
