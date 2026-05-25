using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class CorruptionService(
    CorruptionDbContext db,
    IMaskingService masking,
    ISlaService slaService,
    INotificationService notificationService,
    IAuditService auditService) : ICorruptionService
{
    public async Task<CorruptionReport> SubmitAsync(SubmitCorruptionRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var refNum = await GenerateReferenceNumberAsync(ct);

        var report = new CorruptionReport
        {
            ReferenceNumber = refNum,
            ReporterNameEncrypted = masking.Encrypt(request.ReporterName),
            ReporterPhoneEncrypted = masking.Encrypt(request.ReporterPhone),
            ReporterEmailEncrypted = request.ReporterEmail is null ? null : masking.Encrypt(request.ReporterEmail),
            ReporterIdCardEncrypted = masking.Encrypt(request.ReporterIdCard),
            ReporterNameMasked = masking.MaskName(request.ReporterName),
            ReporterPhoneMasked = masking.MaskPhone(request.ReporterPhone),
            ReporterEmailMasked = masking.MaskEmail(request.ReporterEmail),
            SubjectType = request.SubjectType,
            SubjectPersonName = request.SubjectPersonName,
            SubjectDepartment = request.SubjectDepartment,
            IncidentDate = request.IncidentDate,
            Description = request.Description,
            Priority = "Normal",
            Status = "Pending",
            SlaDeadline = slaService.CalculateDeadline("Normal", now),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Reports.Add(report);
        await db.SaveChangesAsync(ct);

        await auditService.LogAsync("CorruptionReportSubmitted", null, null, "CorruptionReport", report.Id.ToString(), new { refNum }, null, ct);

        await notificationService.SendAsync("ComplaintReceived", request.ReporterPhone, request.ReporterEmail, new()
        {
            ["ReferenceNumber"] = refNum,
            ["TrackingUrl"] = $"https://www.railway.co.th/complaint/track/{refNum}"
        }, ct);

        return report;
    }

    public async Task<CorruptionReport?> GetByReferenceAsync(string referenceNumber, CancellationToken ct = default)
        => await db.Reports.FirstOrDefaultAsync(r => r.ReferenceNumber == referenceNumber, ct);

    public async Task<CorruptionReport?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Reports
            .Include(r => r.InvestigationLogs)
            .Include(r => r.DecryptionLogs)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task UpdateStatusAsync(int id, string newStatus, int actorId, string? note, CancellationToken ct = default)
    {
        var report = await db.Reports.FindAsync([id], ct)
            ?? throw new InvalidOperationException("Report not found");
        report.Status = newStatus;
        report.UpdatedAt = DateTime.UtcNow;
        if (newStatus is "Closed") report.ClosedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("UpdateCorruptionStatus", actorId, null, "CorruptionReport", id.ToString(), new { newStatus }, null, ct);
    }

    public async Task ClaimAsync(int id, int staffId, CancellationToken ct = default)
    {
        var report = await db.Reports.FindAsync([id], ct)
            ?? throw new InvalidOperationException("Report not found");
        if (report.AssignedToId.HasValue) throw new InvalidOperationException("Already claimed");
        report.AssignedToId = staffId;
        report.AssignedAt = DateTime.UtcNow;
        report.Status = "InProgress";
        report.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("ClaimCorruptionCase", staffId, null, "CorruptionReport", id.ToString(), null, null, ct);
    }

    public async Task CloseAsync(int id, string resolutionNote, int actorId, CancellationToken ct = default)
        => await UpdateStatusAsync(id, "Closed", actorId, resolutionNote, ct);

    public async Task<DecryptedReporterInfo> DecryptReporterInfoAsync(int reportId, int requestedById, string reason, string ipAddress, CancellationToken ct = default)
    {
        var report = await db.Reports.FindAsync([reportId], ct)
            ?? throw new InvalidOperationException("Report not found");

        db.DecryptionLogs.Add(new DecryptionLog
        {
            ReportId = reportId,
            RequestedById = requestedById,
            Reason = reason,
            IpAddress = ipAddress,
            RequestedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("DecryptReporterInfo", requestedById, null, "CorruptionReport", reportId.ToString(), new { reason }, ipAddress, ct);

        return new DecryptedReporterInfo(
            masking.Decrypt(report.ReporterNameEncrypted),
            masking.Decrypt(report.ReporterPhoneEncrypted),
            report.ReporterEmailEncrypted is null ? null : masking.Decrypt(report.ReporterEmailEncrypted),
            masking.Decrypt(report.ReporterIdCardEncrypted)
        );
    }

    public async Task<IReadOnlyList<CorruptionReport>> GetQueueAsync(int page, int pageSize, CancellationToken ct = default)
        => await db.Reports
            .OrderBy(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    private async Task<string> GenerateReferenceNumberAsync(CancellationToken ct)
    {
        var thYear = DateTime.UtcNow.Year + 543;
        var count = await db.Reports.CountAsync(ct) + 1;
        return $"COR-{thYear}-{count:D5}";
    }
}
