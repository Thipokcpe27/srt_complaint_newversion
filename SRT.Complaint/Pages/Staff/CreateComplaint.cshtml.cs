#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Staff;

[Authorize(Policy = "StaffOnly")]
public class CreateComplaintModel(
    IComplaintService complaintService,
    AppDbContext db) : PageModel
{
    [BindProperty]
    public CreateComplaintInput Input { get; set; } = new();

    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = [];
    public string SubCategoriesJson { get; private set; } = "{}";

    public static readonly IReadOnlyList<SelectListItem> ChannelOptions =
    [
        new("โทรศัพท์ (Call Center)", "Phone"),
        new("Facebook", "Facebook"),
        new("Line", "Line"),
        new("อีเมล", "Email"),
        new("ยื่นด้วยตนเอง (Walk-in)", "Walkin"),
        new("ช่องทางอื่น", "Other"),
    ];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "สร้างเรื่องร้องเรียนใหม่";
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDataAsync();

        if (!ModelState.IsValid)
            return Page();

        var request = new SubmitComplaintRequest(
            Input.ReporterName,
            Input.ReporterPhone,
            string.IsNullOrWhiteSpace(Input.ReporterEmail) ? null : Input.ReporterEmail,
            null,
            Input.CategoryId,
            Input.SubCategoryId > 0 ? Input.SubCategoryId : null,
            string.IsNullOrWhiteSpace(Input.SubjectStation) ? null : Input.SubjectStation,
            Input.IncidentDate.HasValue ? DateOnly.FromDateTime(Input.IncidentDate.Value) : null,
            Input.Description,
            [],
            Input.Channel
        );

        var complaint = await complaintService.SubmitAsync(request);

        var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await complaintService.ClaimAsync(complaint.Id, staffId);

        TempData["Success"] = $"สร้างเรื่องร้องเรียนสำเร็จ เลขที่อ้างอิง: {complaint.ReferenceNumber}";
        return RedirectToPage("/Staff/CaseDetail", new { id = complaint.Id });
    }

    private async Task LoadDataAsync()
    {
        CategoryOptions = await db.ComplaintCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();

        var subs = await db.ComplaintSubCategories
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .Select(s => new { s.CategoryId, s.Id, s.Name })
            .ToListAsync();

        var grouped = subs.GroupBy(s => s.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(s => new { s.Id, s.Name }).ToList());

        SubCategoriesJson = JsonSerializer.Serialize(grouped);
    }
}

public class CreateComplaintInput
{
    [Required(ErrorMessage = "กรุณาเลือกช่องทาง")]
    public string Channel { get; set; } = "Phone";

    [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
    [MaxLength(200)]
    public string ReporterName { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
    [MaxLength(20)]
    public string ReporterPhone { get; set; } = string.Empty;

    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    public string? ReporterEmail { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกประเภทเรื่อง")]
    [Range(1, int.MaxValue, ErrorMessage = "กรุณาเลือกประเภทเรื่อง")]
    public int CategoryId { get; set; }

    public int? SubCategoryId { get; set; }

    [MaxLength(200)]
    public string? SubjectStation { get; set; }

    public DateTime? IncidentDate { get; set; }

    [Required(ErrorMessage = "กรุณากรอกรายละเอียดคำร้อง")]
    [MinLength(10, ErrorMessage = "รายละเอียดต้องมีอย่างน้อย 10 ตัวอักษร")]
    public string Description { get; set; } = string.Empty;
}
