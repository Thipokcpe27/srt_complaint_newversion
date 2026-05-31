using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages;

public class MaintenanceModel(ISystemSettingService settings) : PageModel
{
    public string Message     { get; private set; } = "ระบบอยู่ระหว่างปรับปรุง กรุณากลับมาใหม่ในภายหลัง";
    public string ExpectedBack { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var n = await settings.GetGroupAsync("maintenance");
        Message      = n.GetValueOrDefault("maintenance.message",       Message);
        ExpectedBack = n.GetValueOrDefault("maintenance.expected_back", "");
    }
}
