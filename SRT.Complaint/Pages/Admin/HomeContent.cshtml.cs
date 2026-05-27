#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class HomeContentModel(IContentBlockService contentService) : PageModel
{
    public ContentBlock Steps   { get; private set; } = ContentBlockService.Defaults.Steps();
    public ContentBlock Contact { get; private set; } = ContentBlockService.Defaults.Contact();
    public ContentBlock Trust   { get; private set; } = ContentBlockService.Defaults.Trust();

    [BindProperty] public BlockInput StepsInput   { get; set; } = new();
    [BindProperty] public BlockInput ContactInput { get; set; } = new();
    [BindProperty] public BlockInput TrustInput   { get; set; } = new();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "จัดการเนื้อหาหน้าแรก";
        var blocks = await contentService.GetHomeBlocksAsync();

        Steps   = blocks.GetValueOrDefault("home_steps",   ContentBlockService.Defaults.Steps());
        Contact = blocks.GetValueOrDefault("home_contact", ContentBlockService.Defaults.Contact());
        Trust   = blocks.GetValueOrDefault("home_trust",   ContentBlockService.Defaults.Trust());

        StepsInput   = new BlockInput { Title = Steps.Title,   Body = Steps.BodyHtml };
        ContactInput = new BlockInput { Title = Contact.Title, Body = Contact.BodyHtml };
        TrustInput   = new BlockInput { Title = Trust.Title,   Body = Trust.BodyHtml };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var staffId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        await contentService.SaveAsync("home_steps",   StepsInput.Title,   StepsInput.Body,   staffId);
        await contentService.SaveAsync("home_contact", ContactInput.Title, ContactInput.Body, staffId);
        await contentService.SaveAsync("home_trust",   TrustInput.Title,   TrustInput.Body,   staffId);

        TempData["Success"] = "บันทึกเนื้อหาหน้าแรกเรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public class BlockInput
    {
        [Required(ErrorMessage = "กรุณาระบุหัวข้อ")]
        [MaxLength(200, ErrorMessage = "หัวข้อยาวเกินไป")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "กรุณาระบุเนื้อหา")]
        public string Body { get; set; } = "";
    }
}
