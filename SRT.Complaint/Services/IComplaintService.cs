using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface IComplaintService
{
    Task<Models.Complaint> SubmitAsync(SubmitComplaintRequest request, CancellationToken ct = default);
    Task<Models.Complaint?> GetByReferenceAsync(string referenceNumber, CancellationToken ct = default);
    Task<Models.Complaint?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateStatusAsync(int id, string newStatus, int actorId, string? note, CancellationToken ct = default);
    Task ClaimAsync(int id, int staffId, CancellationToken ct = default);
    Task TransferAsync(int id, int toOfficerId, string reason, int actorId, CancellationToken ct = default);
    Task CloseAsync(int id, string resolutionNote, int actorId, CancellationToken ct = default);
    Task<IReadOnlyList<Models.Complaint>> GetQueueAsync(ComplaintQueueFilter filter, CancellationToken ct = default);
}

public record SubmitComplaintRequest(
    string ReporterName,
    string ReporterPhone,
    string? ReporterEmail,
    int CategoryId,
    string? SubjectStation,
    DateOnly? IncidentDate,
    string Description,
    IReadOnlyList<IFormFile> Attachments
);

public record ComplaintQueueFilter(
    string? Status = null,
    int? CategoryId = null,
    string? Priority = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? AssignedToId = null,
    int Page = 1,
    int PageSize = 20
);
