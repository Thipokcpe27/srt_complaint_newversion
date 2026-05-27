#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SRT.Complaint.Pages.Public;

public class CorruptionSubmittedModel : PageModel
{
    public string? ReferenceNumber { get; private set; }

    public IActionResult OnGet()
    {
        ViewData["Title"] = "ส่งเรื่องสำเร็จ";
        ReferenceNumber = TempData["CorruptionRef"] as string;
        if (string.IsNullOrEmpty(ReferenceNumber))
            return RedirectToPage("/Public/SubmitCorruption");
        return Page();
    }
}
