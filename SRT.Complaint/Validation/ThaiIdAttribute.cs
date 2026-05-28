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

        return ValidationResult.Success;
    }
}
