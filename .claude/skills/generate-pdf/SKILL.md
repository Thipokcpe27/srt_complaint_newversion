# Generate PDF for eDOC

สร้าง PDF export service สำหรับส่งต่อ eDOC

## Usage

```
/generate-pdf ModelName
```

Example: `/generate-pdf Complaint`

## Steps

1. สร้าง `Services/Pdf/I{ModelName}PdfService.cs` interface
2. สร้าง `Services/Pdf/{ModelName}PdfService.cs` implementation
3. ใช้ QuestPDF สร้าง layout ตาม spec
4. เพิ่ม method `GeneratePdfAsync(int id)` return byte[]
5. สำหรับเรื่องทุจริต: ใช้ชื่อ Masked version
6. Register service ใน Program.cs
7. สร้าง endpoint สำหรับ download PDF
8. Test กับ sample data

## Template (QuestPDF)

```csharp
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace SRT.Complaint.Services.Pdf;

public class {ModelName}PdfService : I{ModelName}PdfService
{
    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var model = await GetModelAsync(id);
        
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                page.Header()
                    .Text("การรถไฟแห่งประเทศไทย")
                    .FontSize(16)
                    .Bold();
                    
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Item().Text($"เลขที่: {model.ReferenceNumber}");
                        col.Item().Text($"วันที่: {model.CreatedAt:dd/MM/yyyy}");
                        // TODO: Add more fields
                    });
                    
                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("พิมพ์เมื่อ: ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
            });
        }).GeneratePdf();
    }
}
```
