using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface ITermsService
{
    Task<ComplaintTerms?> GetTermsAsync();
    Task SaveTermsAsync(string title, string content, bool isActive, int updatedById);
}
