using System.ComponentModel.DataAnnotations;

namespace SRT.Complaint.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class ThaiIdAttribute : ValidationAttribute
{
    public ThaiIdAttribute() : base("เลขบัตรประชาชนไม่ถูกต้อง") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is not string id || string.IsNullOrEmpty(id))
            return ValidationResult.Success;

        // Strip formatting dashes/spaces if any
        id = id.Replace("-", "").Replace(" ", "");

        if (id.Length != 13 || !id.All(char.IsDigit))
            return new ValidationResult("เลขบัตรประชาชนต้องเป็นตัวเลข 13 หลัก");

        // Thai national ID checksum: Σ d[i]×(13-i) for i=0..11
        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (id[i] - '0') * (13 - i);

        int mod = sum % 11;
        int expected = mod == 0 ? 1 : mod == 1 ? 0 : 11 - mod;

        return expected == (id[12] - '0')
            ? ValidationResult.Success
            : new ValidationResult("เลขบัตรประชาชนไม่ถูกต้อง (กรุณาตรวจสอบตัวเลขอีกครั้ง)");
    }
}
