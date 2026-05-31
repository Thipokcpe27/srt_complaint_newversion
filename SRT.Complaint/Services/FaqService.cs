using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class FaqService(AppDbContext db) : IFaqService
{
    public Task<List<FaqItem>> GetAllAsync(bool activeOnly = false)
    {
        var q = db.FaqItems.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return q.OrderBy(x => x.Category).ThenBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
    }

    public Task<FaqItem?> GetByIdAsync(int id) =>
        db.FaqItems.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<FaqItem> CreateAsync(string question, string answer, string? category, int sortOrder, int updatedById)
    {
        var item = new FaqItem
        {
            Question = question,
            Answer = answer,
            Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedById = updatedById
        };
        db.FaqItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(int id, string question, string answer, string? category, int sortOrder, bool isActive, int updatedById)
    {
        var item = await db.FaqItems.FindAsync(id)
            ?? throw new InvalidOperationException($"FAQ item {id} not found");
        item.Question = question;
        item.Answer = answer;
        item.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        item.SortOrder = sortOrder;
        item.IsActive = isActive;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedById = updatedById;
        await db.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int id, int updatedById)
    {
        var item = await db.FaqItems.FindAsync(id)
            ?? throw new InvalidOperationException($"FAQ item {id} not found");
        item.IsActive = !item.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedById = updatedById;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await db.FaqItems.FindAsync(id);
        if (item is not null)
        {
            db.FaqItems.Remove(item);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetCategoriesAsync() =>
        await db.FaqItems
            .Where(x => x.Category != null)
            .Select(x => x.Category!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
}
