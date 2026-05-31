using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Staff;

public class LogoutModel(IAuditService audit) : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Staff/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        var actorId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var actorCode = User.FindFirstValue(ClaimTypes.Name);
        var ip        = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        await HttpContext.SignOutAsync("Cookies");

        await audit.LogAsync("Logout",
            actorId is not null ? int.Parse(actorId) : null,
            actorCode, null, null, null, ip, userAgent, "Success");

        return RedirectToPage("/Staff/Login");
    }
}
