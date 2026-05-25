using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface IPdfExportService
{
    byte[] GenerateComplaintPdf(Models.Complaint complaint, string? officerName = null);
    byte[] GenerateCorruptionReportPdf(CorruptionReport report, string? officerName = null);
}
