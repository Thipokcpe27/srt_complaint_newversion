using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SRT.Complaint.Pages.Staff;

public class LogoutModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Staff/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToPage("/Staff/Login");
    }
}
