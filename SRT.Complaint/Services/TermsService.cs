using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class TermsService(AppDbContext db) : ITermsService
{
    private static readonly HtmlSanitizer Sanitizer = new();
    public async Task<ComplaintTerms?> GetTermsAsync()
    {
        var terms = await db.ComplaintTerms.OrderBy(t => t.Id).FirstOrDefaultAsync();
        if (terms is null)
        {
            terms = new ComplaintTerms
            {
                Title = "หลักเกณฑ์การรับเรื่องร้องเรียน",
                Content = DefaultContent,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            };
            db.ComplaintTerms.Add(terms);
            await db.SaveChangesAsync();
        }
        return terms;
    }

    public async Task SaveTermsAsync(string title, string content, bool isActive, int updatedById)
    {
        var terms = await db.ComplaintTerms.OrderBy(t => t.Id).FirstOrDefaultAsync();
        if (terms is null)
        {
            terms = new ComplaintTerms();
            db.ComplaintTerms.Add(terms);
        }
        terms.Title = title;
        terms.Content = Sanitizer.Sanitize(content);
        terms.IsActive = isActive;
        terms.UpdatedAt = DateTime.UtcNow;
        terms.UpdatedById = updatedById > 0 ? updatedById : null;
        await db.SaveChangesAsync();
    }

    private static readonly string DefaultContent = @"<div class=""space-y-4 text-sm"">
  <p class=""font-medium text-gray-800 bg-blue-50 border border-blue-100 rounded-lg px-3 py-2"">
    เรื่องร้องเรียนที่จะรับพิจารณาต้องมีลักษณะดังต่อไปนี้
  </p>

  <div>
    <p class=""font-semibold text-gray-900 mb-2"">1. ใช้ถ้อยคำหรือข้อความที่สุภาพ และต้องมี</p>
    <ul class=""list-disc pl-6 space-y-1.5 text-gray-700"">
      <li>วัน เดือน ปี</li>
      <li>ชื่อ ที่อยู่ หมายเลขโทรศัพท์ หรืออีเมลที่สามารถติดต่อถึงผู้ร้องเรียนหรือร้องทุกข์ได้</li>
      <li>ข้อเท็จจริง หรือพฤติการณ์ของเรื่องที่ร้องเรียนได้อย่างชัดเจนว่าได้รับความเดือดร้อนหรือเสียหายอย่างไร ต้องการให้แก้ไขดำเนินการอย่างไร หรือชี้ช่องทางแจ้งเบาะแสเกี่ยวกับการทุจริตของเจ้าหน้าที่ หน่วยงานของกรมฯ ได้ชัดแจ้งเพียงพอที่สามารถดำเนินการสืบสวน สอบสวนได้</li>
      <li>ระบุพยาน เอกสาร พยานวัตถุ และพยานบุคคล (ถ้ามี)</li>
    </ul>
  </div>

  <p class=""text-gray-700""><span class=""font-semibold text-gray-900"">2.</span> ข้อร้องเรียนต้องเป็นเรื่องจริงที่มีมูลเหตุ มิได้หวังสร้างกระแสหรือสร้างข่าวที่เสียหายต่อบุคคลอื่นหรือหน่วยงานต่างๆ ที่เกี่ยวข้อง</p>

  <p class=""text-gray-700""><span class=""font-semibold text-gray-900"">3.</span> การใช้บริการร้องเรียนของการรถไฟฯ นั้น ต้องสามารถติดต่อกลับไปยังผู้ใช้บริการได้ เพื่อยืนยันว่ามีตัวตนจริง ไม่ได้สร้างเรื่องเพื่อกล่าวหาบุคคลอื่นหรือหน่วยงานต่างๆ ให้เกิดความเสียหาย</p>

  <p class=""text-gray-700""><span class=""font-semibold text-gray-900"">4.</span> เป็นเรื่องที่ผู้ร้องได้รับความเดือดร้อน หรือเสียหาย อันเนื่องมาจากการปฏิบัติหน้าที่ต่างๆ ของเจ้าหน้าที่หรือหน่วยงานภายในสังกัดของการรถไฟแห่งประเทศไทย</p>

  <p class=""text-gray-700""><span class=""font-semibold text-gray-900"">5.</span> เป็นเรื่องที่ประสงค์ขอให้การรถไฟฯ ช่วยเหลือหรือขจัดความเดือดร้อน ในด้านที่เกี่ยวข้องกับความรับผิดชอบหรือภารกิจของการรถไฟฯ โดยตรง</p>

  <p class=""text-gray-700""><span class=""font-semibold text-gray-900"">6.</span> ข้อร้องเรียนที่มีข้อมูลไม่ครบถ้วน ไม่เพียงพอ หรือไม่สามารถหาข้อมูลเพิ่มเติมได้ในการดำเนินการตรวจสอบ สืบสวน สอบสวนข้อเท็จจริง ตามรายละเอียดที่กล่าวมาในข้อที่ 1 ให้ยุติเรื่องและเก็บเป็นฐานข้อมูล</p>

  <div>
    <p class=""font-semibold text-gray-900 mb-2"">7. ไม่เป็นข้อร้องเรียนที่เข้าลักษณะดังต่อไปนี้</p>
    <ul class=""list-disc pl-6 space-y-1.5 text-gray-700"">
      <li>ข้อร้องเรียนที่เป็นบัตรสนเท่ห์ เว้นแต่บัตรสนเท่ห์นั้นจะระบุรายละเอียดตามข้อที่ 1 จึงจะรับไว้พิจารณาเป็นการเฉพาะเรื่อง</li>
      <li>ข้อร้องเรียนที่เข้าสู่กระบวนการยุติธรรมแล้ว หรือเป็นเรื่องที่ศาลได้มีคำพิพากษาหรือคำสั่งถึงที่สุดแล้ว</li>
      <li>ข้อร้องเรียนที่เกี่ยวข้องกับสถาบันพระมหากษัตริย์</li>
      <li>ข้อร้องเรียนที่เกี่ยวข้องกับนโยบายของรัฐบาล</li>
      <li>ข้อร้องเรียนที่หน่วยงานอื่นได้ดำเนินการตรวจสอบ พิจารณาวินิจฉัย และมีข้อสรุปผลการพิจารณาเป็นที่เรียบร้อยแล้ว เช่น สำนักงานคณะกรรมการป้องกันและปราบปรามการทุจริตแห่งชาติ (ป.ป.ช.), สำนักงานคณะกรรมการป้องกันและปราบปรามการทุจริตภาครัฐ (ป.ป.ท.), สำนักงานป้องกันและปราบปรามการฟอกเงิน (ป.ป.ง.) เป็นต้น</li>
    </ul>
  </div>

  <p class=""text-gray-500 text-xs italic border-t border-gray-100 pt-3"">
    นอกเหนือจากหลักเกณฑ์ดังกล่าวข้างต้นแล้ว ให้อยู่ในดุลยพินิจของผู้บังคับบัญชาว่าจะรับไว้พิจารณาหรือไม่เป็นเรื่องเฉพาะกรณี
  </p>
</div>";
}
