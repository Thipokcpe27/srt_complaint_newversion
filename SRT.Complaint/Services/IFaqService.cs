using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface IFaqService
{
    Task<List<FaqItem>> GetAllAsync(bool activeOnly = false);
    Task<FaqItem?> GetByIdAsync(int id);
    Task<FaqItem> CreateAsync(string question, string answer, string? category, int sortOrder, int updatedById);
    Task UpdateAsync(int id, string question, string answer, string? category, int sortOrder, bool isActive, int updatedById);
    Task ToggleActiveAsync(int id, int updatedById);
    Task DeleteAsync(int id);
    Task<List<string>> GetCategoriesAsync();
}
