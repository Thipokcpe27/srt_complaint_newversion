using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class SystemSettingService(AppDbContext db) : ISystemSettingService
{
    public async Task<string?> GetAsync(string key)
    {
        var setting = await db.SystemSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<string> GetAsync(string key, string defaultValue)
        => await GetAsync(key) ?? defaultValue;

    public async Task<Dictionary<string, string>> GetGroupAsync(string group)
    {
        return await db.SystemSettings.AsNoTracking()
            .Where(s => s.Group == group)
            .ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public async Task SaveGroupAsync(string group, Dictionary<string, string> values, int updatedById)
    {
        var existing = await db.SystemSettings
            .Where(s => s.Group == group)
            .ToListAsync();

        foreach (var (key, value) in values)
        {
            var setting = existing.FirstOrDefault(s => s.Key == key);
            if (setting is null)
            {
                db.SystemSettings.Add(new SystemSetting
                {
                    Key = key,
                    Value = value,
                    Group = group,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = updatedById
                });
            }
            else
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedById = updatedById;
            }
        }

        await db.SaveChangesAsync();
    }
}
