namespace SRT.Complaint.Services;

public interface ISlaService
{
    DateTime CalculateDeadline(string priority, DateTime from);
}
