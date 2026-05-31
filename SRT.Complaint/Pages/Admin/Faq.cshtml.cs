using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class FaqModel(IFaqService faqService) : PageModel
{
    public List<FaqItem> Items { get; set; } = [];
    public List<string> ExistingCategories { get; set; } = [];

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)] public int? EditId { get; set; }
    public FaqItem? EditTarget { get; set; }

    public async Task OnGetAsync()
    {
        Items = await faqService.GetAllAsync();
        ExistingCategories = await faqService.GetCategoriesAsync();
        if (EditId.HasValue)
            EditTarget = await faqService.GetByIdAsync(EditId.Value);
    }

    public async Task<IActionResult> OnPostCreateAsync(
        string newQuestion, string newAnswer, string? newCategory, int newSortOrder)
    {
        if (string.IsNullOrWhiteSpace(newQuestion) || string.IsNullOrWhiteSpace(newAnswer))
        {
            ErrorMessage = "กรุณากรอกคำถามและคำตอบ";
            return RedirectToPage();
        }
        var userId = GetUserId();
        await faqService.CreateAsync(newQuestion, newAnswer, newCategory, newSortOrder, userId);
        SuccessMessage = "เพิ่ม FAQ เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(
        int editFaqId, string editQuestion, string editAnswer, string? editCategory,
        int editSortOrder, bool editIsActive)
    {
        if (string.IsNullOrWhiteSpace(editQuestion) || string.IsNullOrWhiteSpace(editAnswer))
        {
            ErrorMessage = "กรุณากรอกคำถามและคำตอบ";
            return RedirectToPage();
        }
        var userId = GetUserId();
        await faqService.UpdateAsync(editFaqId, editQuestion, editAnswer, editCategory,
            editSortOrder, editIsActive, userId);
        SuccessMessage = "แก้ไข FAQ เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var userId = GetUserId();
        await faqService.ToggleActiveAsync(id, userId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await faqService.DeleteAsync(id);
        SuccessMessage = "ลบ FAQ เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    private int GetUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
