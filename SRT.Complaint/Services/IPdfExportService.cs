using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface IPdfExportService
{
    byte[] GenerateComplaintPdf(Models.Complaint complaint, string? officerName = null, bool maskReporter = false);
    byte[] GenerateCorruptionReportPdf(CorruptionReport report, string? officerName = null);
}
