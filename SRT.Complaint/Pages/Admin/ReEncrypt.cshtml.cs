#nullable enable
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Admin;

[Authorize(Policy = "SuperAdmin")]
public class ReEncryptModel(
    AppDbContext appDb,
    CorruptionDbContext corrDb,
    IMaskingService masking,
    ILogger<ReEncryptModel> logger) : PageModel
{
    [BindProperty]
    public string OldKeyB64 { get; set; } = string.Empty;

    public int StaffCount { get; private set; }
    public int CorruptionCount { get; private set; }
    public List<string> Results { get; private set; } = [];
    public bool Done { get; private set; }

    public async Task OnGetAsync()
    {
        StaffCount = await appDb.StaffUsers.CountAsync(u => u.TempPasswordEncrypted != null);
        CorruptionCount = await corrDb.Reports.CountAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(OldKeyB64))
        {
            ModelState.AddModelError(nameof(OldKeyB64), "กรุณาใส่ Old Key");
            await OnGetAsync();
            return Page();
        }

        byte[] oldKey;
        try { oldKey = Convert.FromBase64String(OldKeyB64.Trim()); }
        catch
        {
            ModelState.AddModelError(nameof(OldKeyB64), "Old Key ไม่ใช่ Base64 ที่ถูกต้อง");
            await OnGetAsync();
            return Page();
        }

        // ── StaffUsers.TempPasswordEncrypted ──
        var staffRows = await appDb.StaffUsers
            .Where(u => u.TempPasswordEncrypted != null)
            .ToListAsync();

        var staffOk = 0;
        foreach (var user in staffRows)
        {
            try
            {
                var plain = DecryptWithKey(user.TempPasswordEncrypted!, oldKey);
                user.TempPasswordEncrypted = masking.Encrypt(plain);
                staffOk++;
            }
            catch (Exception ex)
            {
                Results.Add($"[ERROR] StaffUser {user.Id}: {ex.Message}");
            }
        }
        await appDb.SaveChangesAsync();
        Results.Add($"StaffUsers: {staffOk}/{staffRows.Count} รายการ สำเร็จ");

        // ── CorruptionReports PII ──
        var corrRows = await corrDb.Reports.ToListAsync();

        var corrOk = 0;
        foreach (var report in corrRows)
        {
            try
            {
                if (report.ReporterNameEncrypted.Length > 0)
                    report.ReporterNameEncrypted = masking.Encrypt(DecryptWithKey(report.ReporterNameEncrypted, oldKey));
                if (report.ReporterPhoneEncrypted.Length > 0)
                    report.ReporterPhoneEncrypted = masking.Encrypt(DecryptWithKey(report.ReporterPhoneEncrypted, oldKey));
                if (report.ReporterEmailEncrypted is { Length: > 0 } email)
                    report.ReporterEmailEncrypted = masking.Encrypt(DecryptWithKey(email, oldKey));
                if (report.ReporterIdCardEncrypted.Length > 0)
                    report.ReporterIdCardEncrypted = masking.Encrypt(DecryptWithKey(report.ReporterIdCardEncrypted, oldKey));
                corrOk++;
            }
            catch (Exception ex)
            {
                Results.Add($"[ERROR] CorruptionReport {report.Id}: {ex.Message}");
            }
        }
        await corrDb.SaveChangesAsync();
        Results.Add($"CorruptionReports: {corrOk}/{corrRows.Count} รายการ สำเร็จ");

        logger.LogWarning("Re-encryption completed by {User}: Staff={S} Corruption={C}",
            User.Identity?.Name, staffOk, corrOk);

        Done = true;
        return Page();
    }

    private static string DecryptWithKey(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[data.Length - iv.Length];
        Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(data, iv.Length, cipher, 0, cipher.Length);
        aes.IV = iv;
        using var dec = aes.CreateDecryptor();
        return Encoding.UTF8.GetString(dec.TransformFinalBlock(cipher, 0, cipher.Length));
    }
}
