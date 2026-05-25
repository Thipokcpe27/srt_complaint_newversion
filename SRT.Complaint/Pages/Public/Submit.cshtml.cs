using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Public;

public class SubmitModel(IComplaintService complaintService, AppDbContext db) : PageModel
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private const int MaxFileCount = 5;

    [BindProperty]
    public ComplaintSubmitViewModel Input { get; set; } = new();

    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();

        if (!ValidateFiles(out var fileError))
        {
            ModelState.AddModelError("Input.Attachments", fileError);
        }

        if (!ModelState.IsValid)
            return Page();

        var request = new SubmitComplaintRequest(
            Input.ReporterName,
            Input.ReporterPhone,
            string.IsNullOrWhiteSpace(Input.ReporterEmail) ? null : Input.ReporterEmail,
            Input.CategoryId,
            string.IsNullOrWhiteSpace(Input.SubjectStation) ? null : Input.SubjectStation,
            Input.IncidentDate.HasValue ? DateOnly.FromDateTime(Input.IncidentDate.Value) : null,
            Input.Description,
            Input.Attachments ?? []
        );

        var complaint = await complaintService.SubmitAsync(request);

        TempData["Success"] = $"ยื่นเรื่องร้องเรียนสำเร็จ เลขที่อ้างอิง: {complaint.ReferenceNumber}";
        return RedirectToPage("/Public/Track", new { refNo = complaint.ReferenceNumber });
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await db.ComplaintCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();

        CategoryOptions = categories;
    }

    private bool ValidateFiles(out string error)
    {
        error = string.Empty;
        var files = Input.Attachments;

        if (files == null || files.Count == 0)
            return true;

        if (files.Count > MaxFileCount)
        {
            error = $"แนบไฟล์ได้ไม่เกิน {MaxFileCount} ไฟล์";
            return false;
        }

        foreach (var file in files)
        {
            if (file.Length > MaxFileSizeBytes)
            {
                error = $"ไฟล์ \"{file.FileName}\" มีขนาดเกิน 10 MB";
                return false;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                error = $"ไฟล์ \"{file.FileName}\" ไม่รองรับประเภทไฟล์นี้ (รองรับ: JPG, PNG, PDF, DOC, DOCX)";
                return false;
            }
        }

        return true;
    }
}

public class ComplaintSubmitViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
    [StringLength(200, ErrorMessage = "ชื่อ-นามสกุลต้องไม่เกิน 200 ตัวอักษร")]
    [Display(Name = "ชื่อ-นามสกุล")]
    public string ReporterName { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
    [RegularExpression(@"^0[0-9]{8,9}$", ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง (ตัวอย่าง: 0812345678)")]
    [Display(Name = "เบอร์โทรศัพท์")]
    public string ReporterPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    [StringLength(200)]
    [Display(Name = "อีเมล")]
    public string? ReporterEmail { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกประเภทเรื่อง")]
    [Display(Name = "ประเภทเรื่อง")]
    public int CategoryId { get; set; }

    [StringLength(200, ErrorMessage = "ชื่อสถานีต้องไม่เกิน 200 ตัวอักษร")]
    [Display(Name = "สถานี / จุดที่เกิดเหตุ")]
    public string? SubjectStation { get; set; }

    [Display(Name = "วันที่เกิดเหตุ")]
    [DataType(DataType.Date)]
    public DateTime? IncidentDate { get; set; }

    [Required(ErrorMessage = "กรุณากรอกรายละเอียดเรื่องร้องเรียน")]
    [MinLength(20, ErrorMessage = "กรุณากรอกรายละเอียดอย่างน้อย 20 ตัวอักษร")]
    [StringLength(4000, ErrorMessage = "รายละเอียดต้องไม่เกิน 4,000 ตัวอักษร")]
    [Display(Name = "รายละเอียดเรื่องร้องเรียน")]
    public string Description { get; set; } = string.Empty;

    public List<IFormFile>? Attachments { get; set; }
}
