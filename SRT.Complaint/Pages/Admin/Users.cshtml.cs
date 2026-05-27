#nullable enable
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;
using SRT.Complaint.Services;


namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class UsersModel(AppDbContext db, IAuditService auditService, IMaskingService masking) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search      { get; set; }
    [BindProperty(SupportsGet = true)] public string? RoleFilter  { get; set; }
    [BindProperty(SupportsGet = true)] public int     CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int?    EditId      { get; set; }

    public const int PageSize = 20;
    public IReadOnlyList<StaffUser> Users  { get; private set; } = [];
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public int PageStart  => TotalCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int PageEnd    => Math.Min(CurrentPage * PageSize, TotalCount);
    public StaffUser? EditTarget { get; private set; }

    // Create
    [BindProperty] public string? NewEmployeeCode { get; set; }
    [BindProperty] public string? NewFullName     { get; set; }
    [BindProperty] public string? NewEmail        { get; set; }
    [BindProperty] public string? NewRole         { get; set; }

    // Edit
    [BindProperty] public int     EditUserId   { get; set; }
    [BindProperty] public string? EditFullName { get; set; }
    [BindProperty] public string? EditEmail    { get; set; }
    [BindProperty] public string? EditRole     { get; set; }
    [BindProperty] public bool    EditIsActive { get; set; }

    // Reset password
    [BindProperty] public int ResetUserId { get; set; }

    // View password
    [BindProperty] public int ViewPasswordUserId { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "จัดการเจ้าหน้าที่";
        await LoadListAsync();
        if (EditId.HasValue)
            EditTarget = await db.StaffUsers.FindAsync(EditId.Value);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var code = NewEmployeeCode?.Trim() ?? "";
        if (code.Length != 7 || !code.All(char.IsDigit))
        {
            TempData["Error"] = "รหัสพนักงานต้องเป็นตัวเลข 7 หลัก";
            return RedirectToPage();
        }
        if (string.IsNullOrWhiteSpace(NewFullName) || string.IsNullOrWhiteSpace(NewRole))
        {
            TempData["Error"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
            return RedirectToPage();
        }
        if (await db.StaffUsers.AnyAsync(u => u.EmployeeCode == code))
        {
            TempData["Error"] = $"รหัสพนักงาน {code} มีในระบบแล้ว";
            return RedirectToPage();
        }

        var tempPassword = GenerateTempPassword();
        var expiresAt = DateTime.UtcNow.AddHours(8);
        var user = new StaffUser
        {
            EmployeeCode              = code,
            FullName                  = NewFullName.Trim(),
            Email                     = string.IsNullOrWhiteSpace(NewEmail) ? null : NewEmail.Trim(),
            Role                      = NewRole,
            PasswordHash              = BCrypt.Net.BCrypt.HashPassword(tempPassword, workFactor: 12),
            IsActive                  = true,
            MustChangePassword        = true,
            TempPasswordExpiresAt     = expiresAt,
            TempPasswordEncrypted     = masking.Encrypt(tempPassword),
            CreatedAt                 = DateTime.UtcNow
        };
        db.StaffUsers.Add(user);
        await db.SaveChangesAsync();
        await auditService.LogAsync("CreateStaffUser", GetActorId(), GetActorCode(),
            "StaffUser", user.Id.ToString(), new { user.EmployeeCode, user.FullName, user.Role }, GetIp());

        TempData["Success"]       = $"สร้างบัญชี {user.FullName} ({code}) เรียบร้อยแล้ว";
        TempData["TempPassword"]  = tempPassword;
        TempData["TempPwTarget"]  = $"{user.FullName} ({code})";
        TempData["TempPwExpires"] = expiresAt.ToLocalTime().ToString("HH:mm น. (dd/MM/yyyy)");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (string.IsNullOrWhiteSpace(EditFullName) || string.IsNullOrWhiteSpace(EditRole))
        {
            TempData["Error"] = "กรุณากรอกชื่อและบทบาท";
            return RedirectToPage(new { editId = EditUserId });
        }
        var user = await db.StaffUsers.FindAsync(EditUserId);
        if (user == null) return NotFound();

        user.FullName = EditFullName.Trim();
        user.Email    = string.IsNullOrWhiteSpace(EditEmail) ? null : EditEmail.Trim();
        user.Role     = EditRole;
        user.IsActive = EditIsActive;
        await db.SaveChangesAsync();
        await auditService.LogAsync("EditStaffUser", GetActorId(), GetActorCode(),
            "StaffUser", user.Id.ToString(), new { user.FullName, user.Role, user.IsActive }, GetIp());

        TempData["Success"] = $"แก้ไขข้อมูล {user.FullName} เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync()
    {
        var user = await db.StaffUsers.FindAsync(ResetUserId);
        if (user == null) return NotFound();

        var tempPassword = GenerateTempPassword();
        var expiresAt = DateTime.UtcNow.AddHours(8);
        user.PasswordHash          = BCrypt.Net.BCrypt.HashPassword(tempPassword, workFactor: 12);
        user.MustChangePassword    = true;
        user.TempPasswordExpiresAt = expiresAt;
        user.TempPasswordEncrypted = masking.Encrypt(tempPassword);
        await db.SaveChangesAsync();
        await auditService.LogAsync("ResetPassword", GetActorId(), GetActorCode(),
            "StaffUser", user.Id.ToString(), null, GetIp());

        TempData["Success"]       = $"รีเซ็ตรหัสผ่านของ {user.FullName} เรียบร้อยแล้ว";
        TempData["TempPassword"]  = tempPassword;
        TempData["TempPwTarget"]  = user.FullName;
        TempData["TempPwExpires"] = expiresAt.ToLocalTime().ToString("HH:mm น. (dd/MM/yyyy)");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostViewPasswordAsync()
    {
        var user = await db.StaffUsers.FindAsync(ViewPasswordUserId);
        if (user == null) return NotFound();

        if (!user.MustChangePassword || user.TempPasswordEncrypted == null)
        {
            TempData["Error"] = "ไม่พบรหัสผ่านชั่วคราว (พนักงานอาจเปลี่ยนรหัสผ่านแล้ว หรือยังไม่เคยรีเซ็ต)";
            return RedirectToPage();
        }

        if (user.TempPasswordExpiresAt.HasValue && user.TempPasswordExpiresAt.Value < DateTime.UtcNow)
        {
            TempData["Error"] = "รหัสผ่านชั่วคราวหมดอายุแล้ว กรุณารีเซ็ตรหัสผ่านใหม่";
            return RedirectToPage();
        }

        var tempPassword = masking.Decrypt(user.TempPasswordEncrypted);
        TempData["TempPassword"]  = tempPassword;
        TempData["TempPwTarget"]  = $"{user.FullName} ({user.EmployeeCode})";
        TempData["TempPwExpires"] = user.TempPasswordExpiresAt!.Value.ToLocalTime().ToString("HH:mm น. (dd/MM/yyyy)");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var user = await db.StaffUsers.FindAsync(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        await db.SaveChangesAsync();
        await auditService.LogAsync(user.IsActive ? "ActivateStaffUser" : "DeactivateStaffUser",
            GetActorId(), GetActorCode(), "StaffUser", id.ToString(), null, GetIp());
        TempData["Success"] = $"{(user.IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน")} {user.FullName} เรียบร้อย";
        return RedirectToPage(new { Search, RoleFilter, CurrentPage });
    }

    private async Task LoadListAsync()
    {
        var q = db.StaffUsers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim();
            q = q.Where(u => u.FullName.Contains(s) || u.EmployeeCode.Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(RoleFilter))
            q = q.Where(u => u.Role == RoleFilter);

        TotalCount = await q.CountAsync();
        TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
        if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

        Users = await q
            .OrderBy(u => u.EmployeeCode)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    private int    GetActorId()   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetActorCode() => User.FindFirstValue("EmployeeCode") ?? "";
    private string GetIp()        => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GenerateTempPassword()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        return new string(Enumerable.Range(0, 8)
            .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
            .ToArray());
    }

    public static string RoleLabel(string r) => r switch
    {
        "GeneralOfficer"    => "เจ้าหน้าที่ทั่วไป",
        "CorruptionOfficer" => "เจ้าหน้าที่ทุจริต",
        "SuperAdmin"        => "ผู้ดูแลระบบ",
        _                   => r
    };
}
