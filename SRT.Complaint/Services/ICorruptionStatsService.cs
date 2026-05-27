namespace SRT.Complaint.Services;

public interface ICorruptionStatsService
{
    Task<CorruptionSummaryStats> GetSummaryAsync(CancellationToken ct = default);
}

public record CorruptionSummaryStats(
    DateTime AsOf,
    int Total, int Pending, int InProgress, int UnderReview,
    int Closed, int Rejected, int SlaBreached, int TodayNew,
    IReadOnlyList<SubjectTypeStat> BySubjectType);

public record SubjectTypeStat(string SubjectType, int Count);
