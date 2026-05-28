#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Services;
using System.Text.Json.Serialization;

namespace SRT.Complaint.Controllers.Api;

/// <summary>
/// Receives real-time push notifications from Traffy Fondue Exchange API.
///
/// Register webhook URLs with Traffy support:
///   New issue  : POST  https://your-domain/api/traffy-webhook/new-issue
///   Status update: PATCH https://your-domain/api/traffy-webhook/update-status
///
/// Traffy signs nothing — secure by using a secret token in the path or header.
/// Add TraffyFondue:WebhookSecret in appsettings and compare on every request.
/// </summary>
[ApiController]
[Route("api/traffy-webhook")]
public class TraffyWebhookController(
    IComplaintService complaintService,
    AppDbContext      db,
    IConfiguration    config,
    ILogger<TraffyWebhookController> logger) : ControllerBase
{
    private const string SystemKey = "traffy_fondue";

    // ─── New issue pushed by Traffy ───────────────────────────────
    [HttpPost("new-issue")]
    public async Task<IActionResult> NewIssue(
        [FromBody] TraffyWebhookIssue payload,
        CancellationToken ct)
    {
        if (!ValidateSecret()) return Unauthorized();
        if (string.IsNullOrEmpty(payload.TicketId)) return BadRequest("ticket_id required");

        logger.LogInformation("Traffy webhook: new issue {TicketId}", payload.TicketId);

        var alreadyExists = await db.Complaints
            .AnyAsync(c => c.ExternalSystem == SystemKey && c.ExternalId == payload.TicketId, ct);

        if (alreadyExists)
        {
            logger.LogDebug("Traffy webhook: {TicketId} already imported — skip", payload.TicketId);
            return Ok(new { imported = false, reason = "duplicate" });
        }

        var dto = MapToDto(payload);

        var defaultCategory = await db.ComplaintCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .FirstOrDefaultAsync(ct);

        if (defaultCategory == null)
        {
            logger.LogError("Traffy webhook: no active category found");
            return StatusCode(500, "No active category");
        }

        var request = new SubmitComplaintRequest(
            ReporterName:   dto.ReporterName ?? $"นำเข้าจาก {SystemKey}",
            ReporterPhone:  dto.ReporterPhone ?? "-",
            ReporterEmail:  null,
            ReporterIdCard: null,
            CategoryId:     defaultCategory.Id,
            SubCategoryId:  null,
            SubjectStation: dto.Address,
            IncidentDate:   dto.IncidentDate.HasValue
                                ? DateOnly.FromDateTime(dto.IncidentDate.Value)
                                : null,
            Description:    dto.Description,
            Attachments:    Array.Empty<IFormFile>()
        );

        var complaint = await complaintService.SubmitAsync(request, ct);
        complaint.ExternalSystem = SystemKey;
        complaint.ExternalId     = payload.TicketId;
        complaint.Channel        = SystemKey;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Traffy webhook: imported {TicketId} → {Ref}",
            payload.TicketId, complaint.ReferenceNumber);

        return Ok(new { imported = true, reference = complaint.ReferenceNumber });
    }

    // ─── Status update pushed by Traffy ──────────────────────────
    [HttpPatch("update-status")]
    public async Task<IActionResult> UpdateStatus(
        [FromBody] TraffyWebhookStatusUpdate payload,
        CancellationToken ct)
    {
        if (!ValidateSecret()) return Unauthorized();
        if (string.IsNullOrEmpty(payload.TicketId)) return BadRequest("ticket_id required");

        logger.LogInformation("Traffy webhook: status update {TicketId} → status_id={StatusId}",
            payload.TicketId, payload.StatusId);

        var complaint = await db.Complaints
            .FirstOrDefaultAsync(c => c.ExternalSystem == SystemKey && c.ExternalId == payload.TicketId, ct);

        if (complaint == null)
        {
            logger.LogDebug("Traffy webhook: {TicketId} not found in SRT system — ignore", payload.TicketId);
            return Ok(new { updated = false, reason = "not_found" });
        }

        // Only update if Traffy marks it as resolved/invalid
        var newSrtStatus = payload.StatusId switch
        {
            3 => "Resolved",
            4 => "Rejected",
            _ => null
        };

        if (newSrtStatus != null && complaint.Status != newSrtStatus)
        {
            var oldStatus = complaint.Status;
            complaint.Status    = newSrtStatus;
            complaint.UpdatedAt = DateTime.UtcNow;
            if (newSrtStatus is "Resolved" or "Closed")
                complaint.ClosedAt = DateTime.UtcNow;

            // Record as a system note (AuthorId = 0 = system)
            db.ComplaintNotes.Add(new Models.ComplaintNote
            {
                ComplaintId = complaint.Id,
                AuthorId    = null,
                NoteType    = "StatusChange",
                Content     = $"{oldStatus}→{newSrtStatus} (Traffy: {payload.Note})",
                CreatedAt   = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }

        return Ok(new { updated = true });
    }

    // ─── Helpers ──────────────────────────────────────────────────
    private bool ValidateSecret()
    {
        var secret = config["TraffyFondue:WebhookSecret"];
        if (string.IsNullOrEmpty(secret)) return true; // not configured → open (dev only)

        // Accept secret via header X-Traffy-Secret or query ?secret=
        var headerVal = Request.Headers["X-Traffy-Secret"].FirstOrDefault();
        var queryVal  = Request.Query["secret"].FirstOrDefault();
        return headerVal == secret || queryVal == secret;
    }

    private static ExternalComplaintDto MapToDto(TraffyWebhookIssue i) => new(
        ExternalId:    i.TicketId,
        Description:   i.Description ?? "(ไม่มีรายละเอียด)",
        Address:       i.Address,
        CategoryHint:  i.Topic?.FirstOrDefault() ?? i.Type,
        IncidentDate:  string.IsNullOrEmpty(i.Timestamp) ? null
                       : DateTime.TryParse(i.Timestamp, out var dt) ? dt : null,
        ReporterName:  i.Name,
        ReporterPhone: NormalisePhone(i.Phone),
        RawStatus:     i.Status ?? "รอรับเรื่อง");

    private static string? NormalisePhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone)) return null;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length is >= 9 and <= 10 ? digits : null;
    }

    // ─── Payload DTOs ─────────────────────────────────────────────
    public sealed class TraffyWebhookIssue
    {
        [JsonPropertyName("ticket_id")]    public string  TicketId    { get; set; } = "";
        [JsonPropertyName("description")]  public string? Description { get; set; }
        [JsonPropertyName("address")]      public string? Address     { get; set; }
        [JsonPropertyName("type")]         public string? Type        { get; set; }
        [JsonPropertyName("topic")]        public List<string>? Topic { get; set; }
        [JsonPropertyName("timestamp")]    public string? Timestamp   { get; set; }
        [JsonPropertyName("status")]       public string? Status      { get; set; }
        [JsonPropertyName("name")]         public string? Name        { get; set; }
        [JsonPropertyName("phone")]        public string? Phone       { get; set; }
    }

    public sealed class TraffyWebhookStatusUpdate
    {
        [JsonPropertyName("ticket_id")]  public string TicketId { get; set; } = "";
        [JsonPropertyName("status_id")]  public int    StatusId { get; set; }
        [JsonPropertyName("note")]       public string? Note    { get; set; }
    }
}
