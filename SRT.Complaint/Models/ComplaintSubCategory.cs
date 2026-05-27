#nullable enable
namespace SRT.Complaint.Models;

public class ComplaintSubCategory
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ComplaintCategory Category { get; set; } = null!;
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
