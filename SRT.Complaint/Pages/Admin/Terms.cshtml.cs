using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class TermsModel(ITermsService termsService) : PageModel
{
    public ComplaintTerms? CurrentTerms { get; private set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        CurrentTerms = await termsService.GetTermsAsync();
        if (CurrentTerms is not null)
        {
            Input = new InputModel
            {
                Title = CurrentTerms.Title,
                Content = CurrentTerms.Content,
                IsActive = CurrentTerms.IsActive
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            CurrentTerms = await termsService.GetTermsAsync();
            return Page();
        }

        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        await termsService.SaveTermsAsync(Input.Title, Input.Content, Input.IsActive, userId);
        TempData["Success"] = "บันทึกหลักเกณฑ์เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "กรุณาระบุหัวข้อ")]
        [MaxLength(300, ErrorMessage = "หัวข้อยาวเกินไป")]
        public string Title { get; set; } = "หลักเกณฑ์การรับเรื่องร้องเรียน";

        [Required(ErrorMessage = "กรุณาระบุเนื้อหา")]
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
