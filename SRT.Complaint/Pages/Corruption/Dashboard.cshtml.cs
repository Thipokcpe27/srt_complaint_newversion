using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Corruption;

[Authorize(Policy = "CorruptionAccess")]
public class DashboardModel(ICorruptionService corruptionService) : PageModel
{
    public CorruptionDashboardStats Stats { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "แดชบอร์ดทุจริต";
        Stats = await corruptionService.GetDashboardStatsAsync();
    }
}
