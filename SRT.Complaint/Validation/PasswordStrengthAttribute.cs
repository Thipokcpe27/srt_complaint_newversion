#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SRT.Complaint.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed partial class PasswordStrengthAttribute : ValidationAttribute
{
    [GeneratedRegex(@"[฀-๿]")]
    private static partial Regex ThaiCharRegex();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || password.Length == 0)
            return new ValidationResult("กรุณากรอกรหัสผ่าน");

        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("อย่างน้อย 8 ตัวอักษร");

        if (ThaiCharRegex().IsMatch(password))
            errors.Add("ห้ามใช้อักษรภาษาไทย");

        if (!password.Any(char.IsUpper))
            errors.Add("ตัวพิมพ์ใหญ่ (A-Z) อย่างน้อย 1 ตัว");

        if (!password.Any(char.IsLower))
            errors.Add("ตัวพิมพ์เล็ก (a-z) อย่างน้อย 1 ตัว");

        if (!password.Any(char.IsDigit))
            errors.Add("ตัวเลข (0-9) อย่างน้อย 1 ตัว");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("อักขระพิเศษ (!@#$...) อย่างน้อย 1 ตัว");

        if (errors.Count > 0)
            return new ValidationResult("รหัสผ่านไม่ผ่านเกณฑ์: " + string.Join(", ", errors));

        return ValidationResult.Success;
    }
}
