namespace SRT.Complaint.Services;

public interface ISystemSettingService
{
    Task<string?> GetAsync(string key);
    Task<string> GetAsync(string key, string defaultValue);
    Task<Dictionary<string, string>> GetGroupAsync(string group);
    Task SaveGroupAsync(string group, Dictionary<string, string> values, int updatedById);
}
