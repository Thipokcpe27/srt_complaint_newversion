using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class PdfExportService : IPdfExportService
{
    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateComplaintPdf(Models.Complaint complaint, string? officerName = null)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontFamily("TH Sarabun New", "Sarabun", "sans-serif").FontSize(14));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("การรถไฟแห่งประเทศไทย").Bold().FontSize(18);
                    col.Item().AlignCenter().Text("บันทึกเรื่องร้องเรียน").FontSize(16);
                    col.Item().PaddingTop(5).LineHorizontal(1);
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("เลขที่คำร้อง:").Bold();
                        r.RelativeItem(3).Text(complaint.ReferenceNumber);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("วันที่รับเรื่อง:").Bold();
                        r.RelativeItem(3).Text(complaint.CreatedAt.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("th-TH")));
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ประเภทเรื่อง:").Bold();
                        r.RelativeItem(3).Text(complaint.Category?.Name ?? "-");
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ความเร่งด่วน:").Bold();
                        r.RelativeItem(3).Text(complaint.Priority);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("กำหนดส่งผล:").Bold();
                        r.RelativeItem(3).Text(complaint.SlaDeadline?.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("th-TH")) ?? "-");
                    });

                    col.Item().PaddingTop(10).Text("ข้อมูลผู้ร้องเรียน:").Bold();
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ชื่อ-สกุล:").Bold();
                        r.RelativeItem(3).Text(complaint.ReporterName);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("เบอร์โทร:").Bold();
                        r.RelativeItem(3).Text(complaint.ReporterPhone);
                    });
                    if (!string.IsNullOrEmpty(complaint.ReporterEmail))
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("อีเมล:").Bold();
                            r.RelativeItem(3).Text(complaint.ReporterEmail);
                        });

                    if (!string.IsNullOrEmpty(complaint.SubjectStation))
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("สถานที่/สถานี:").Bold();
                            r.RelativeItem(3).Text(complaint.SubjectStation);
                        });
                    if (complaint.IncidentDate.HasValue)
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("วัน/เวลาเกิดเหตุ:").Bold();
                            r.RelativeItem(3).Text(complaint.IncidentDate.Value.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("th-TH")));
                        });

                    col.Item().PaddingTop(10).Text("รายละเอียดคำร้อง:").Bold();
                    col.Item().Text(complaint.Description);

                    if (!string.IsNullOrEmpty(officerName))
                        col.Item().PaddingTop(10).Row(r =>
                        {
                            r.RelativeItem().Text("เจ้าหน้าที่รับเรื่อง:").Bold();
                            r.RelativeItem(3).Text(officerName);
                        });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("วันที่พิมพ์:").Bold();
                        r.RelativeItem(3).Text(DateTime.Now.ToString("dd MMMM yyyy เวลา HH:mm น.", new System.Globalization.CultureInfo("th-TH")));
                    });
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("หน้า ");
                    t.CurrentPageNumber();
                    t.Span(" จาก ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateCorruptionReportPdf(CorruptionReport report, string? officerName = null)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontFamily("TH Sarabun New", "Sarabun", "sans-serif").FontSize(14));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("การรถไฟแห่งประเทศไทย").Bold().FontSize(18);
                    col.Item().AlignCenter().Text("บันทึกรับแจ้งเบาะแสทุจริต (ลับ)").FontSize(16);
                    col.Item().PaddingTop(5).LineHorizontal(1);
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("เลขที่คำร้อง:").Bold();
                        r.RelativeItem(3).Text(report.ReferenceNumber);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ผู้แจ้ง:").Bold();
                        r.RelativeItem(3).Text(report.ReporterNameMasked);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("เบอร์โทร:").Bold();
                        r.RelativeItem(3).Text(report.ReporterPhoneMasked);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ประเภทการทุจริต:").Bold();
                        r.RelativeItem(3).Text(report.SubjectType);
                    });
                    col.Item().PaddingTop(10).Text("รายละเอียด:").Bold();
                    col.Item().Text(report.Description);

                    if (!string.IsNullOrEmpty(officerName))
                        col.Item().PaddingTop(10).Row(r =>
                        {
                            r.RelativeItem().Text("เจ้าหน้าที่รับเรื่อง:").Bold();
                            r.RelativeItem(3).Text(officerName);
                        });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("วันที่พิมพ์:").Bold();
                        r.RelativeItem(3).Text(DateTime.Now.ToString("dd MMMM yyyy เวลา HH:mm น.", new System.Globalization.CultureInfo("th-TH")));
                    });
                });
            });
        }).GeneratePdf();
    }
}
