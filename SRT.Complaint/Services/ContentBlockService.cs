#nullable enable
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class ContentBlockService(AppDbContext db) : IContentBlockService
{
    private static readonly HtmlSanitizer Sanitizer = new();
    public async Task<Dictionary<string, ContentBlock>> GetHomeBlocksAsync()
    {
        var blocks = await db.ContentBlocks
            .Where(b => b.Key.StartsWith("home_"))
            .ToDictionaryAsync(b => b.Key);

        if (!blocks.ContainsKey("home_steps"))
            blocks["home_steps"] = Defaults.Steps();
        if (!blocks.ContainsKey("home_contact"))
            blocks["home_contact"] = Defaults.Contact();
        if (!blocks.ContainsKey("home_trust"))
            blocks["home_trust"] = Defaults.Trust();

        return blocks;
    }

    public async Task SaveAsync(string key, string title, string bodyHtml, int staffId)
    {
        var block = await db.ContentBlocks.FirstOrDefaultAsync(b => b.Key == key);
        if (block is null)
        {
            block = new ContentBlock { Key = key };
            db.ContentBlocks.Add(block);
        }
        block.Title = title;
        block.BodyHtml = Sanitizer.Sanitize(bodyHtml);
        block.UpdatedById = staffId > 0 ? staffId : null;
        block.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // Default content shown before admin saves anything
    public static class Defaults
    {
        public static ContentBlock Steps() => new()
        {
            Key = "home_steps",
            Title = "ขั้นตอนการร้องเรียน",
            IsActive = true,
            BodyHtml =
                """
                <ol class="step-list">
                  <li><b>ยื่นเรื่องออนไลน์</b><p>กรอกแบบฟอร์มและแนบเอกสารที่เกี่ยวข้อง ระบบออกเลขที่อ้างอิงให้ทันที</p></li>
                  <li><b>รับเรื่องและแจ้งยืนยัน</b><p>เจ้าหน้าที่รับเรื่องและแจ้งกลับผ่าน SMS หรืออีเมลที่ท่านระบุไว้</p></li>
                  <li><b>ตรวจสอบข้อเท็จจริง</b><p>สืบสวนและประสานงานกับหน่วยงานที่เกี่ยวข้อง ท่านสามารถติดตามความคืบหน้าได้ตลอดเวลา</p></li>
                  <li class="done"><b>แจ้งผลการพิจารณา</b><p>แจ้งผลให้ท่านทราบพร้อมมาตรการที่ดำเนินการ</p></li>
                </ol>
                <p class="sla-note">ระยะเวลาดำเนินการไม่เกิน <strong>15 วันทำการ</strong></p>
                """
        };

        public static ContentBlock Contact() => new()
        {
            Key = "home_contact",
            Title = "ช่องทางติดต่อ",
            IsActive = true,
            BodyHtml =
                """
                <dl class="contact-dl">
                  <div><dt>สายด่วน</dt><dd>1690</dd></div>
                  <div><dt>อีเมล</dt><dd>complaint@railway.co.th</dd></div>
                  <div><dt>เวลาทำการ</dt><dd>จันทร์ – ศุกร์ &nbsp;08:30 – 16:30 น.</dd></div>
                </dl>
                """
        };

        public static ContentBlock Trust() => new()
        {
            Key = "home_trust",
            Title = "ความปลอดภัย & ความเป็นส่วนตัว",
            IsActive = true,
            BodyHtml =
                """
                <ul class="trust-ul">
                  <li>ข้อมูลของท่านได้รับการปกป้องตาม <strong>พ.ร.บ. คุ้มครองข้อมูลส่วนบุคคล พ.ศ. 2562</strong> (PDPA)</li>
                  <li>ข้อมูลผู้แจ้งเบาะแสทุจริตเข้ารหัส <strong>AES-256</strong> เปิดเผยได้เฉพาะเจ้าหน้าที่ผู้มีสิทธิ์เท่านั้น</li>
                  <li>ทุกเรื่องได้รับ <strong>เลขที่อ้างอิง</strong> สำหรับติดตามสถานะตลอดกระบวนการ</li>
                  <li>การเชื่อมต่อผ่าน <strong>HTTPS</strong> เข้ารหัสทุกการรับส่งข้อมูล</li>
                </ul>
                <p class="pdpa-note">หากมีข้อสงสัยเรื่องการคุ้มครองข้อมูลส่วนบุคคล กรุณาติดต่อเจ้าหน้าที่คุ้มครองข้อมูล (DPO) ที่ dpo@railway.co.th</p>
                """
        };
    }
}
