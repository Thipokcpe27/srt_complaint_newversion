namespace SRT.Complaint.Services;

public interface IStatsService
{
    Task<ComplaintSummaryStats> GetSummaryAsync(CancellationToken ct = default);
    Task<ComplaintDetailedStats> GetDetailedAsync(CancellationToken ct = default);
}

public record ComplaintSummaryStats(
    DateTime AsOf,
    int Total, int Pending, int InProgress, int Resolved,
    int Closed, int Rejected, int SlaBreached, int TodayNew);

public record ComplaintDetailedStats(
    DateTime AsOf,
    IReadOnlyList<CategoryStat> ByCategory,
    IReadOnlyList<PriorityStat> ByPriority,
    IReadOnlyList<StatusStat> ByStatus,
    double AverageResolutionHours);

public record CategoryStat(string Category, int Total, int Open);
public record PriorityStat(string Priority, int Count);
public record StatusStat(string Status, int Count);
