using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class ComplaintService(
    AppDbContext db,
    ISlaService slaService,
    INotificationService notificationService,
    IAuditService auditService,
    IConfiguration config) : IComplaintService
{
    public async Task<Models.Complaint> SubmitAsync(SubmitComplaintRequest request, CancellationToken ct = default)
    {
        var category = await db.ComplaintCategories.FindAsync([request.CategoryId], ct)
            ?? throw new InvalidOperationException("Category not found");

        var priority = category.DefaultPriority;
        var now = DateTime.UtcNow;
        var refNum = await GenerateReferenceNumberAsync(ct);

        var complaint = new Models.Complaint
        {
            ReferenceNumber = refNum,
            ReporterName = request.ReporterName,
            ReporterPhone = request.ReporterPhone,
            ReporterEmail = request.ReporterEmail,
            CategoryId = request.CategoryId,
            SubjectStation = request.SubjectStation,
            IncidentDate = request.IncidentDate,
            Description = request.Description,
            Priority = priority,
            Status = "Pending",
            SlaDeadline = slaService.CalculateDeadline(priority, now),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Complaints.Add(complaint);
        await db.SaveChangesAsync(ct);

        await SaveAttachmentsAsync(complaint.Id, request.Attachments, ct);
        await auditService.LogAsync("ComplaintSubmitted", null, null, "Complaint", complaint.Id.ToString(), new { refNum }, null, ct);

        await notificationService.SendAsync("ComplaintReceived", request.ReporterPhone, request.ReporterEmail, new()
        {
            ["ReferenceNumber"] = refNum,
            ["TrackingUrl"] = $"https://www.railway.co.th/complaint/track/{refNum}"
        }, ct);

        return complaint;
    }

    public async Task<Models.Complaint?> GetByReferenceAsync(string referenceNumber, CancellationToken ct = default)
        => await db.Complaints
            .Include(c => c.Category)
            .Include(c => c.AssignedTo)
            .Include(c => c.Attachments)
            .Include(c => c.Notes)
            .FirstOrDefaultAsync(c => c.ReferenceNumber == referenceNumber, ct);

    public async Task<Models.Complaint?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Complaints
            .Include(c => c.Category)
            .Include(c => c.AssignedTo)
            .Include(c => c.Attachments)
            .Include(c => c.Notes).ThenInclude(n => n.Author)
            .Include(c => c.TransferLogs)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task UpdateStatusAsync(int id, string newStatus, int actorId, string? note, CancellationToken ct = default)
    {
        var complaint = await db.Complaints.FindAsync([id], ct)
            ?? throw new InvalidOperationException("Complaint not found");
        var oldStatus = complaint.Status;
        complaint.Status = newStatus;
        complaint.UpdatedAt = DateTime.UtcNow;
        if (newStatus is "Closed" or "Resolved") complaint.ClosedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(note))
            db.ComplaintNotes.Add(new ComplaintNote { ComplaintId = id, AuthorId = actorId, NoteType = "PublicReply", Content = note, CreatedAt = DateTime.UtcNow });

        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("UpdateStatus", actorId, null, "Complaint", id.ToString(), new { oldStatus, newStatus }, null, ct);

        await notificationService.SendAsync("StatusChanged", complaint.ReporterPhone, complaint.ReporterEmail, new()
        {
            ["ReferenceNumber"] = complaint.ReferenceNumber,
            ["Status"] = newStatus
        }, ct);
    }

    public async Task ClaimAsync(int id, int staffId, CancellationToken ct = default)
    {
        var complaint = await db.Complaints.FindAsync([id], ct)
            ?? throw new InvalidOperationException("Complaint not found");
        if (complaint.AssignedToId.HasValue) throw new InvalidOperationException("Already claimed");

        complaint.AssignedToId = staffId;
        complaint.AssignedAt = DateTime.UtcNow;
        complaint.Status = "InProgress";
        complaint.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("ClaimCase", staffId, null, "Complaint", id.ToString(), null, null, ct);
    }

    public async Task TransferAsync(int id, int toOfficerId, string reason, int actorId, CancellationToken ct = default)
    {
        var complaint = await db.Complaints.FindAsync([id], ct)
            ?? throw new InvalidOperationException("Complaint not found");

        db.ComplaintTransferLogs.Add(new ComplaintTransferLog
        {
            ComplaintId = id,
            FromOfficerId = complaint.AssignedToId,
            ToOfficerId = toOfficerId,
            Reason = reason,
            TransferredAt = DateTime.UtcNow
        });

        complaint.AssignedToId = toOfficerId;
        complaint.AssignedAt = DateTime.UtcNow;
        complaint.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("TransferCase", actorId, null, "Complaint", id.ToString(), new { toOfficerId, reason }, null, ct);
    }

    public async Task CloseAsync(int id, string resolutionNote, int actorId, CancellationToken ct = default)
        => await UpdateStatusAsync(id, "Closed", actorId, resolutionNote, ct);

    public async Task<IReadOnlyList<Models.Complaint>> GetQueueAsync(ComplaintQueueFilter filter, CancellationToken ct = default)
    {
        var q = db.Complaints.Include(c => c.Category).Include(c => c.AssignedTo).AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status)) q = q.Where(c => c.Status == filter.Status);
        if (filter.CategoryId.HasValue) q = q.Where(c => c.CategoryId == filter.CategoryId);
        if (!string.IsNullOrEmpty(filter.Priority)) q = q.Where(c => c.Priority == filter.Priority);
        if (filter.FromDate.HasValue) q = q.Where(c => c.CreatedAt >= filter.FromDate);
        if (filter.ToDate.HasValue) q = q.Where(c => c.CreatedAt <= filter.ToDate);
        if (filter.AssignedToId.HasValue) q = q.Where(c => c.AssignedToId == filter.AssignedToId);

        return await q.OrderByDescending(c => c.Priority == "Critical")
            .ThenByDescending(c => c.Priority == "Urgent")
            .ThenBy(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);
    }

    private async Task<string> GenerateReferenceNumberAsync(CancellationToken ct)
    {
        var thYear = DateTime.UtcNow.Year + 543;
        var count = await db.Complaints.CountAsync(ct) + 1;
        return $"GEN-{thYear}-{count:D5}";
    }

    private async Task SaveAttachmentsAsync(int complaintId, IReadOnlyList<IFormFile> files, CancellationToken ct)
    {
        var storagePath = config["FileUpload:StoragePath"] ?? Path.GetTempPath();
        foreach (var file in files)
        {
            if (file.Length == 0) continue;
            var storedName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(storagePath, storedName);
            await using var fs = File.Create(fullPath);
            await file.CopyToAsync(fs, ct);
            db.ComplaintAttachments.Add(new ComplaintAttachment
            {
                ComplaintId = complaintId,
                FileName = file.FileName,
                StoredPath = fullPath,
                FileSize = file.Length,
                MimeType = file.ContentType,
                UploadedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync(ct);
    }
}
