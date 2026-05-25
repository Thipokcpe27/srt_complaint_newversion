using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface ICorruptionService
{
    Task<CorruptionReport> SubmitAsync(SubmitCorruptionRequest request, CancellationToken ct = default);
    Task<CorruptionReport?> GetByReferenceAsync(string referenceNumber, CancellationToken ct = default);
    Task<CorruptionReport?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateStatusAsync(int id, string newStatus, int actorId, string? note, CancellationToken ct = default);
    Task ClaimAsync(int id, int staffId, CancellationToken ct = default);
    Task CloseAsync(int id, string resolutionNote, int actorId, CancellationToken ct = default);
    Task<DecryptedReporterInfo> DecryptReporterInfoAsync(int reportId, int requestedById, string reason, string ipAddress, CancellationToken ct = default);
    Task<IReadOnlyList<CorruptionReport>> GetQueueAsync(int page, int pageSize, CancellationToken ct = default);
}

public record SubmitCorruptionRequest(
    string ReporterName,
    string ReporterPhone,
    string? ReporterEmail,
    string ReporterIdCard,
    string SubjectType,
    string? SubjectPersonName,
    string? SubjectDepartment,
    DateOnly? IncidentDate,
    string Description,
    IReadOnlyList<IFormFile> Attachments
);

public record DecryptedReporterInfo(
    string Name,
    string Phone,
    string? Email,
    string IdCard
);
