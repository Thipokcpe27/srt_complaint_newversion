namespace SRT.Complaint.Models;

public class ComplaintTerms
{
    public int Id { get; set; }
    public string Title { get; set; } = "หลักเกณฑ์การรับเรื่องร้องเรียน";
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }
    public StaffUser? UpdatedBy { get; set; }
}
