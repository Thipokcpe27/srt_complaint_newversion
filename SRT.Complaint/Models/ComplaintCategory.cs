#nullable enable
namespace SRT.Complaint.Models;

public class ComplaintCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string DefaultPriority { get; set; } = "Normal";
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public ICollection<ComplaintSubCategory> SubCategories { get; set; } = new List<ComplaintSubCategory>();
}
