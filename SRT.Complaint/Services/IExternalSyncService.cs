#nullable enable
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface IExternalSyncService
{
    IReadOnlyList<IExternalSystemAdapter> GetAvailableSystems();
    Task<ExternalSyncLog> SyncAsync(string systemKey, int triggeredById, CancellationToken ct = default);
    Task<IReadOnlyList<ExternalSyncLog>> GetRecentLogsAsync(int count = 20, CancellationToken ct = default);
}
