# Progress — ระบบรับเรื่องร้องเรียน รฟท.

## ✅ เสร็จแล้ว

### Phase 1: Core Infrastructure
- [x] Models ทั้งหมด (Complaint, StaffUser, Category, SLA, ฯลฯ)
- [x] AppDbContext + CorruptionDbContext + Migrations
- [x] Services Layer (ComplaintService, SlaService, NotificationService, ฯลฯ)
- [x] Program.cs — Auth, DI, Rate Limiting, Middleware
- [x] _Layout.cshtml (Public) + _StaffLayout.cshtml
- [x] SlaBackgroundService — ตรวจ SLA breach ทุก 30 นาที

### Phase 2: Staff Pages
- [x] Pages/Staff/Login.cshtml + .cs — เข้าสู่ระบบ (BCrypt verify)
- [x] Pages/Staff/Logout.cshtml + .cs — ออกจากระบบ
- [x] Pages/Staff/Dashboard.cshtml + .cs — Stat cards, 7-day chart, SLA warnings
- [x] Pages/Staff/Queue.cshtml + .cs — คิวเรื่อง + filter + pagination + claim
- [x] Pages/Staff/CaseDetail.cshtml + .cs — รายละเอียด + claim/note/transfer/close + PDF download
- [x] Pages/Staff/Workload.cshtml + .cs — Workload ทีม + progress bars

### Phase 3: Corruption Track
- [x] Pages/Corruption/Dashboard.cshtml + .cs
- [x] Pages/Corruption/Queue.cshtml + .cs
- [x] Pages/Corruption/CaseDetail.cshtml + .cs — + PDF download + Decrypt reporter

### Phase 4: Admin Pages
- [x] Pages/Admin/Dashboard.cshtml + .cs
- [x] Pages/Admin/Users.cshtml + .cs — staff CRUD
- [x] Pages/Admin/Categories.cshtml + .cs
- [x] Pages/Admin/SlaSettings.cshtml + .cs
- [x] Pages/Admin/Notifications.cshtml + .cs
- [x] Pages/Admin/ApiKeys.cshtml + .cs — create/revoke, raw key shown once + Usage link
- [x] Pages/Admin/AuditLog.cshtml + .cs — paginated + filter + Excel export
- [x] Pages/Admin/Reports.cshtml + .cs — Chart.js charts + ClosedXML Excel export
- [x] Pages/Admin/Webhooks.cshtml + .cs — ดู/ลบ webhook ที่ลงทะเบียนผ่าน API
- [x] Pages/Admin/ApiKeyUsage.cshtml + .cs — ดู request log ของแต่ละ API key

### Phase 5: Public Pages
- [x] Pages/Index.cshtml — หน้าแรก + Quick track form ใน hero section
- [x] Pages/Public/Submit.cshtml + .cs — ยื่นเรื่องร้องเรียนทั่วไป + Rate limiting
- [x] Pages/Public/Track.cshtml + .cs — ติดตามสถานะ (GEN- และ COR-) + satisfaction form
- [x] Pages/Public/SubmitCorruption.cshtml + .cs — แจ้งเบาะแสทุจริต
- [x] Pages/Public/CorruptionSubmitted.cshtml + .cs — หน้ายืนยันหลังส่งเรื่องทุจริต

### Phase 6: API Controllers
- [x] Filters/ApiKeyAuthFilter.cs — X-API-Key auth + IP whitelist + rate limit
- [x] Filters/RequireScopeAttribute.cs — scope enforcement
- [x] Controllers/Api/ComplaintsController.cs — GET/POST/edoc-payload + GET status + PUT status
- [x] Controllers/Api/StatsController.cs — GET /api/stats/summary + /api/stats/detailed
- [x] Controllers/Api/CorruptionStatsController.cs — GET /api/stats/corruption
- [x] Controllers/Api/WebhooksController.cs — GET/POST/DELETE /api/webhooks
- [x] Services/IApiRequestLogService.cs + ApiRequestLogService.cs

### Phase 7: PDF + Email + SMS
- [x] PdfExportService — QuestPDF (GenerateComplaintPdf + GenerateCorruptionReportPdf)
- [x] NotificationService — MailKit + SMS Gateway

### Phase 8: Webhook Infrastructure
- [x] Services/IWebhookService.cs — interface
- [x] Services/WebhookService.cs — HTTP delivery + HMAC-SHA256 signing
- [x] Services/WebhookRetryService.cs — BackgroundService ลอง retry ทุก 5 นาที
- [x] Registered in Program.cs (Scoped + HostedService + HttpClient "Webhook")
- [x] Webhook triggers in ComplaintService (complaint.created, complaint.status_changed, complaint.closed)

### Phase 10: UX Improvements
- [x] AuditLog — แปล Action + Detail เป็นภาษาไทยที่คนทั่วไปอ่านได้ (ทั้งใน table และ Excel export)
- [x] Admin/Users — Modal popup แสดงรหัสผ่านชั่วคราว (ปิดได้เฉพาะกด "รับทราบ") + ปุ่มคัดลอก
- [x] StaffUser.TempPasswordExpiresAt — temp password หมดอายุ 8 ชั่วโมง + migration AddTempPasswordExpiry
- [x] Login — ตรวจสอบ TempPasswordExpiresAt หมดอายุ → แจ้ง error ให้ติดต่อ admin
- [x] ChangePassword — clear TempPasswordExpiresAt เมื่อเปลี่ยนรหัสผ่านสำเร็จ
- [x] Admin/Users — ปุ่ม "ดูรหัสผ่าน" (สีม่วง) สำหรับ user ที่ยังไม่เปลี่ยนรหัสผ่าน (MustChangePassword=true + ยังไม่หมดอายุ) — ใช้ AES decrypt จาก TempPasswordEncrypted field + migration AddTempPasswordEncrypted

### Phase 9: Auth & User Management Fixes
- [x] Bug fix: Admin/Dashboard.cshtml.cs — concurrent EF Core queries บน DbContext เดียว → เปลี่ยนเป็น sequential await
- [x] Feature: MustChangePassword — เพิ่ม field ใน StaffUser + migration AddMustChangePassword
- [x] Feature: Pages/Staff/ChangePassword.cshtml + .cs — หน้าเปลี่ยนรหัสผ่าน (verify current + new + confirm)
- [x] Feature: Login redirect → /Staff/ChangePassword เมื่อ MustChangePassword = true พร้อม claim
- [x] Feature: Middleware ใน Program.cs — บังคับ redirect ทุก route เมื่อ MustChangePassword claim set
- [x] Feature: Admin/Users — auto-generate temp password (8 อักขระ alphanumeric) แทนให้ admin กรอก
- [x] Feature: Admin/Users — Reset Password เป็นปุ่มเดียว auto-generate + set MustChangePassword = true
- [x] Feature: Admin/Users — แสดง TempPassword banner หลัง create/reset

### Phase 13: Reference Number — SQL SEQUENCE + New Format
- [x] Migration AddComplaintSeq: CREATE SEQUENCE dbo.ComplaintSeq (AppDbContext) — applied
- [x] Migration AddCorruptionSeq: CREATE SEQUENCE corruption.CorruptionSeq (CorruptionDbContext) — applied
- [x] ComplaintService: GenerateReferenceNumberAsync → NEXT VALUE FOR dbo.ComplaintSeq → format SRT-COMPL-{year}-{seq:D4}
- [x] CorruptionService: GenerateReferenceNumberAsync → NEXT VALUE FOR corruption.CorruptionSeq → format SRT-CORUPT-{year}-{seq:D4}
- [x] Placeholder อัปเดตทั้ง Index.cshtml และ Track.cshtml
- Note: Sequence ไม่ reset ตามปี — เลขวิ่งต่อเนื่อง (ปลอดภัยกว่า, ไม่มี race condition)
- Note: เรื่องเก่าใน DB ยังเป็น format GEN-/COR- ต้องลบทิ้งหรือปล่อยไว้ (ไม่กระทบระบบ)

### Phase 12: Track Page — Phone Verification + Timeline
- [x] ComplaintNote.AuthorId → nullable (int?) เพื่อรองรับ system-generated notes
- [x] AppDbContext: FK config สำหรับ ComplaintNote.Author → ClientSetNull (NO ACTION)
- [x] Migration: MakeNoteAuthorNullable — applied
- [x] Program.cs: AddSession (30 นาที, HttpOnly, SameSite=Strict) + UseSession
- [x] ComplaintService.UpdateStatusAsync: บันทึก StatusChange note ทุกครั้งที่เปลี่ยนสถานะ
- [x] ComplaintService.ClaimAsync: บันทึก StatusChange note "Pending→InProgress" เมื่อ claim
- [x] Track.cshtml.cs: 2-step phone verification (session-based) + BuildTimeline() + OnPostVerifyAsync
- [x] Track.cshtml: หน้ายืนยันตัวตน (phone 4 ตัวท้าย) + timeline ใหม่ + ข้อความ "ลืม ref ☎ 1690"
- Note: COR- reports ไม่ต้องยืนยัน (ข้อมูลแสดงน้อยอยู่แล้ว + anonymous)
- Note: Timeline format: "OldStatus→NewStatus" เก็บใน ComplaintNote.Content

### Phase 11: Sub-Category (หัวข้อย่อย)
- [x] Models: ComplaintSubCategory (new) + update ComplaintCategory + Complaint
- [x] AppDbContext: DbSet + entity config + FK (SubCategoryId nullable, SetNull on delete)
- [x] Migration: AddSubCategories
- [x] IComplaintService: SubmitComplaintRequest เพิ่ม SubCategoryId?
- [x] ComplaintService: set SubCategoryId บน complaint
- [x] Submit.cshtml.cs: โหลด sub-categories เป็น JSON grouped by categoryId + SubCategoryId ใน ViewModel
- [x] Submit.cshtml: dropdown หัวข้อย่อย show/hide ด้วย JS ตาม category ที่เลือก
- [x] Admin/Categories.cshtml + .cs: แสดง sub-categories ใต้แต่ละ category + CRUD (create/edit/toggle/delete)

### Phase 15: Track Page — Timeline + Satisfaction UX
- [x] Track.cshtml: Status Stepper — 4 ขั้น (รับเรื่อง/ดำเนินการ/พิจารณา/เสร็จสิ้น) พร้อม checkmark/X icon และ progress line
- [x] Track.cshtml: Timeline redesign — icon สี per event type, vertical line, reply content ใน blue box
- [x] Track.cshtml: Satisfaction form fix — เปลี่ยนจาก radio (ไม่ visible) เป็น `<button>` ที่ควบคุมด้วย JS, hidden input รับค่า, client-side validate ก่อน submit, hover preview + score label

### Phase 14: Password Validation
- [x] Validation/PasswordStrengthAttribute.cs — custom DataAnnotation attribute ตรวจสอบ: ห้ามไทย, ≥8 ตัว, ตัวพิมพ์ใหญ่, พิมพ์เล็ก, ตัวเลข, อักขระพิเศษ
- [x] ChangePassword.cshtml.cs — แทน [MinLength(8)] ด้วย [PasswordStrength]
- [x] ChangePassword.cshtml — เพิ่ม real-time checklist แสดงเงื่อนไขรหัสผ่านขณะพิมพ์ (JS oninput)

### Phase 16: Form Validation & Input Masks
- [x] Validation/ThaiIdAttribute.cs — ตรวจ checksum เลขบัตรประชาชน 13 หลัก (Σd[i]×(13-i) mod 11)
- [x] Models/Complaint.cs — เพิ่ม ReporterIdCard? (nullable)
- [x] AppDbContext.cs — HasMaxLength(20) สำหรับ ReporterIdCard
- [x] IComplaintService / ComplaintService — เพิ่ม ReporterIdCard? ใน SubmitComplaintRequest
- [x] Submit.cshtml.cs — property setter strip dashes สำหรับ Phone + เพิ่ม ReporterIdCard? field ใหม่
- [x] SubmitCorruption.cshtml.cs — property setter strip dashes สำหรับ Phone + ID Card, เปลี่ยน [RegularExpression] เป็น [ThaiId]
- [x] Submit.cshtml — เพิ่ม field เลขบัตรประชาชน (optional) + JS applyPhoneMask / applyIdCardMask + strip on submit capture
- [x] SubmitCorruption.cshtml — JS masks เดียวกัน + อัปเดต placeholder ใส่ dash format
- [x] Migration AddReporterIdCard — applied (dbo.Complaints.ReporterIdCard nvarchar(20) NULL)

### Phase 17: PDPA Cookie Banner + Terms Modal
- [x] Models/ComplaintTerms.cs — Id, Title, Content (nvarchar max), IsActive, UpdatedAt, UpdatedById
- [x] Services/ITermsService.cs + TermsService.cs — GetTermsAsync (auto-seed default content), SaveTermsAsync
- [x] Data/AppDbContext.cs — DbSet<ComplaintTerms> + entity config (ClientSetNull FK)
- [x] Migration: AddComplaintTerms — applied
- [x] Pages/Shared/_TermsModal.cshtml — modal overlay, checkbox, ยอมรับ/ยกเลิก, JS: onTermsAccepted()
- [x] Pages/Shared/_CookieBanner.cshtml — PDPA essential cookies notice, localStorage srt_pdpa_consent
- [x] Pages/Admin/Terms.cshtml + .cs — edit Title/Content (HTML), IsActive toggle, preview tab, demo preview
- [x] Pages/Public/Submit.cshtml + .cs — inject ITermsService, Terms property, modal + form-content-wrapper hidden
- [x] Pages/Public/SubmitCorruption.cshtml + .cs — เดียวกัน (OnGet→OnGetAsync)
- [x] Pages/Shared/_Layout.cshtml — <partial name="_CookieBanner" /> ก่อน </body>
- [x] Pages/Shared/_StaffLayout.cshtml — เพิ่ม "หลักเกณฑ์ร้องเรียน" ใน Admin sidebar
- [x] Program.cs — AddScoped<ITermsService, TermsService>

### Phase 18: External Sync — Traffy Fondue Integration
- [x] Bug: Track.cshtml — RZ1010 `@{}` inside `@if{}` Razor syntax error → ลบ wrapper `@{}` ออก ตัวแปร C# อยู่ตรง ๆ ใน `@if`
- [x] Bug: ThaiIdAttribute — ลบ mod-11 checksum (บัตรเก่าบางใบผ่านไม่ได้) เหลือแค่ตรวจ 13 หลัก
- [x] Bug: jQuery Validate phone/ID card error on blur — เพิ่ม `normalizer` strip non-digits ก่อน validate (Submit.cshtml + SubmitCorruption.cshtml)
- [x] Bug: Track page corruption — prefix check ผิด (`COR-` → `SRT-CORUPT-`) แก้ใน Track.cshtml.cs
- [x] Bug: Corruption CaseDetail status loop — `UnderReview` ไม่มี `Closed` → แก้ AllowedNextStatuses + เพิ่มปุ่ม "ส่งกลับสืบสวน" + `OnPostSendBackAsync`
- [x] Feature: Track.cshtml corruption section — เพิ่ม stepper 4 ขั้น + timeline + status descriptions (ละเอียดขึ้น)
- [x] Feature: Apply migration AddCorruptionSeq (CorruptionDbContext) — `dotnet ef database update --no-build`
- [x] Feature: TraffyFonduAdapter.cs — implement เต็มรูปแบบตาม Traffy Exchange API Spec:
  - JWT auth `POST /get-auth/v1` + token cache in-memory (SemaphoreSlim thread-safe, refresh 60s ก่อนหมด)
  - FetchNewAsync `GET /get-issues/v1?org_id={orgId}&duration=week`
  - PushStatusAsync `PATCH /update-issue/v1` + SRT status → Traffy status_id mapping
  - Phone normalise (strip non-digits, ตรวจ 9–10 หลัก)
- [x] Feature: TraffyWebhookController.cs — real-time push จาก Traffy
  - `POST /api/traffy-webhook/new-issue` → import เรื่องใหม่เข้า DB ทันที (dedup check ก่อน)
  - `PATCH /api/traffy-webhook/update-status` → sync สถานะกลับเข้า complaint (AuthorId null = system)
  - Webhook secret validation via `X-Traffy-Secret` header หรือ query `?secret=`
- [x] Config: appsettings.json เพิ่ม `TraffyFondue` block (ApiUrl, Username, Password, OrgId, WebhookSecret)
- [x] Feature: Staff/Queue.cshtml — ย้ายปุ่ม "ดึงคำร้อง" + modal หน่วยงาน จาก Admin → Staff Queue
  - เจ้าหน้าที่กดดึงได้เองจากหน้าคิวโดยตรง
  - `OnPostSyncAsync` ใน QueueModel — บันทึก log + result banner
  - Admin/ExternalSync ยังอยู่ไว้ดู history log

### Phase 20: ตั้งค่าระบบ (System Settings)
- [x] Models/SystemSetting.cs — Key (PK), Value, Group, Label, Description, UpdatedAt, UpdatedById
- [x] Services/ISystemSettingService.cs + SystemSettingService.cs — Get/GetGroup/SaveGroup
- [x] AppDbContext.cs — DbSet<SystemSetting> + entity config
- [x] Migration: AddSystemSettings + SyncSystemSettingsSnapshot — applied via direct SQL (app lock workaround)
- [x] Pages/Admin/Settings.cshtml + .cs — Group 1: ข้อมูลองค์กร (org.name, org.name_short, org.address, org.phone, org.email, org.website)
- [x] _StaffLayout.cshtml — เพิ่มเมนู "ตั้งค่าระบบ" ใต้ section ตั้งค่าระบบ
- [x] Program.cs — AddScoped<ISystemSettingService, SystemSettingService>
- [x] Group 2: การแจ้งเตือน (Email/SMS) — Tab notify, toggle Email/SMS, SMTP fields, SMS Gateway fields, NotificationService อ่าน DB ก่อน fallback config
- [x] Group 3: ความปลอดภัย & Rate Limit — Tab security, Rate Limit (Submit/Login/TrackVerify), Session Timeout, Program.cs อ่านจาก appsettings.json, บันทึกเขียนกลับ appsettings.json
- [x] Group 4: Maintenance Mode — MaintenanceMiddleware (cache 30s), Pages/Maintenance.cshtml (503), Tab maintenance: toggle+message+expected_back, ล้าง cache ทันทีเมื่อบันทึก
- [x] Group 5: Traffy Fondue — Tab traffy, toggle enable, API URL/OrgId/Username/Password/WebhookSecret, TraffyFonduAdapter อ่าน DB แบบ lazy (cache 60s), InvalidateCache เมื่อบันทึก, token invalidate อัตโนมัติเมื่อ credentials เปลี่ยน

### Phase 19: FAQ (คำถามที่พบบ่อย)
- [x] Models/FaqItem.cs — Id, Question, Answer, Category, SortOrder, IsActive, CreatedAt, UpdatedAt, UpdatedById
- [x] Services/IFaqService.cs + FaqService.cs
- [x] AppDbContext.cs — DbSet<FaqItem> + entity config (SetNull FK → StaffUsers)
- [x] Migration: AddFaqItems — applied (dbo.FaqItems)
- [x] Pages/Admin/Faq.cshtml + .cs — จัดการ FAQ (CRUD)
- [x] Pages/Public/Faq.cshtml + .cs — หน้า FAQ สาธารณะ

### Security Fixes (2026-05-31)
- [x] TraffyWebhookController.ValidateSecretAsync() — แก้ 4 ปัญหาพร้อมกัน:
  - อ่าน secret จาก DB (ISystemSettingService key: traffy.webhook_secret) แทน IConfiguration → แก้ split-brain
  - fail-open (return true เมื่อว่าง) → fail-closed (return false + LogWarning)
  - ลบ query string ?secret= ออก รับแค่ header X-Traffy-Secret → กัน secret รั่วใน log
  - เปลี่ยนจาก == เป็น CryptographicOperations.FixedTimeEquals → กัน timing attack

### Phase 21: Audit Log — พ.ร.บ.คอมพิวเตอร์ 2560 Compliance
- [x] Models/AuditLog.cs — เพิ่ม UserAgent (nvarchar 500) + Outcome (nvarchar 20)
- [x] IAuditService / AuditService — เพิ่ม optional params userAgent + outcome
- [x] Pages/Staff/Login.cshtml.cs — inject IAuditService, log "Login" (Success) + "LoginFailed" (Failed) ทั้ง InvalidCredentials และ TempPasswordExpired
- [x] Pages/Staff/Logout.cshtml.cs — inject IAuditService, log "Logout" (Success)
- [x] Services/AuditLogRetentionService.cs — BackgroundService ลบ log เก่ากว่า N วัน ทุก 24 ชั่วโมง (default 90 วัน จาก AuditLog:RetentionDays ใน appsettings.json)
- [x] Program.cs — register AuditLogRetentionService as HostedService
- [x] appsettings.json — เพิ่ม AuditLog:RetentionDays = 90
- [x] Migration: AddAuditLogFields — applied (dbo.AuditLogs + Outcome + UserAgent)
- [x] Pages/Admin/AuditLog.cshtml — Outcome badge (✓สำเร็จ/✗ล้มเหลว) ใน Action column, UserAgent ใต้ IP
- [x] Pages/Admin/AuditLog.cshtml.cs — Excel export เพิ่ม col ผลลัพธ์ + User Agent, FormatAction Login/LoginFailed/Logout, FormatDetail LoginFailed reason

**ครอบคลุมตาม พ.ร.บ.:** ตัวตนผู้ใช้ ✅ · วัน/เวลา ✅ · IP Address ✅ · Login/Logout/LoginFailed ✅ · UserAgent/Application ✅ · Outcome สำเร็จ/ล้มเหลว ✅ · Retention ≥90 วัน ✅

### Phase 22: PDPA — ลบข้อมูลส่วนตัวผู้ร้องเรียน
- [x] Models/Complaint.cs + CorruptionReport.cs — เพิ่ม PiiDeletedAt (DateTime?)
- [x] Services/PdpaRetentionService.cs — BackgroundService ทุก 24 ชั่วโมง, anonymize PII ทั้ง Complaint + CorruptionReport, static RunNowAsync() สำหรับ manual trigger
- [x] Pages/Admin/PdpaRetention.cshtml + .cs — แสดงสถิติรอลบ/ลบแล้ว, ตั้งค่า retention days, ปุ่ม "ลบทันที"
- [x] SystemSettings: pdpa.complaint_retention_days (default 1825), pdpa.corruption_retention_days (default 1825)
- [x] Program.cs — register PdpaRetentionService as HostedService
- [x] _StaffLayout.cshtml — เพิ่มเมนู "PDPA / ข้อมูลส่วนตัว" ใต้ Audit Log
- [x] Migration: AddComplaintPiiDeletedAt (AppDbContext) + AddCorruptionPiiDeletedAt (CorruptionDbContext) — applied

**Logic ลบ:** ลบเฉพาะ ชื่อ/เบอร์/อีเมล/เลขบัตร (เรื่องและประวัติดำเนินงานยังอยู่) · นับจาก ClosedAt ถ้ามี ไม่งั้นจาก CreatedAt · ขั้นต่ำ 365 วัน

## Notes / Issues พบระหว่างทำ

- ใช้ `@page "{id:int}"` สำหรับ CaseDetail ให้ URL เป็น `/Staff/CaseDetail/123`
- Login.cshtml override layout เป็น `_Layout` (public)
- SlaBackgroundService ใช้ `IServiceScopeFactory` เพราะ DbContext เป็น Scoped service
- `ComplaintQueueFilter` เป็น positional record
- Track.cshtml ใช้ `?Ref=xxx` parameter (case-insensitive bind)
- PDF download ใช้ POST form + AntiForgeryToken เพื่อป้องกัน CSRF
- CorruptionSubmitted: TempData["CorruptionRef"] ถ้าไม่มีให้ redirect กลับ SubmitCorruption
- Track COR- detection: ถ้า ref ขึ้นต้นด้วย "COR-" ให้ query CorruptionService
- Satisfaction form แสดงเฉพาะเมื่อ Status = Closed/Resolved และ SatisfactionScore = null
- Webhook SecretHash เก็บ raw secret (hex 32 bytes) สำหรับ HMAC-SHA256 signing
- Admin/Dashboard: ห้ามใช้ Task.WhenAll + ContinueWith กับ EF Core DbContext เดียวกัน — ใช้ sequential await แทน
- MustChangePassword middleware: ต้อง whitelist /Staff/ChangePassword, /Staff/Logout, /favicon, /css/, /js/, /lib/ เพื่อไม่ให้ redirect loop
- GenerateTempPassword ใช้ RandomNumberGenerator.GetInt32 (thread-safe, crypto-safe) แทน Random
- AuditLog export: ใช้ POST form พร้อม hidden filter fields (SupportsGet = true ยังรับ POST ได้)
- [EnableRateLimiting("SubmitPolicy")] ใช้ที่ method level ใน OnPostAsync
- WebhookRetryService.RetryPendingAsync เรียกผ่าน IServiceScopeFactory scope
- StatsController /detailed ใช้ in-memory average calculation แทน EF.Functions.DateDiffHour

## Tech Stack Reference

- ASP.NET Core 9 Razor Pages
- EF Core 9 + SQL Server
- Tailwind CSS + HTMX
- BCrypt.Net-Next 4.2.0
- QuestPDF + MailKit
- Serilog
- ClosedXML (Excel export — Reports + AuditLog)
- Chart.js 4.x (reports)
- HMAC-SHA256 (webhook signing)
