#nullable enable
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface IContentBlockService
{
    Task<Dictionary<string, ContentBlock>> GetHomeBlocksAsync();
    Task SaveAsync(string key, string title, string bodyHtml, int staffId);
}
