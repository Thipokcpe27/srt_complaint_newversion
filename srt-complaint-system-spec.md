# ระบบรับเรื่องร้องเรียน การรถไฟแห่งประเทศไทย
## Technical Specification — ฉบับละเอียด

**เวอร์ชัน:** 1.1  
**วันที่:** มิถุนายน 2568  
**สถานะ:** Draft for Review

**Changelog:**
- v1.1 — เพิ่มหมวด 17: API Management (API Keys, Scopes, Rate Limiting, Webhooks, Usage Logs)

---

## 1. ภาพรวมระบบ

ระบบรับเรื่องร้องเรียนออนไลน์สำหรับการรถไฟแห่งประเทศไทย (รฟท.) รองรับ 2 ช่องทางหลักคือเรื่องร้องเรียนทั่วไปและเรื่องแจ้งเบาะแสทุจริต โดยแต่ละช่องทางมีกระบวนการ สิทธิ์การเข้าถึง และการจัดเก็บข้อมูลที่แยกจากกันโดยสิ้นเชิง ประชาชนไม่จำเป็นต้องสมัครสมาชิกหรือล็อกอินเพื่อยื่นเรื่อง และสามารถติดตามสถานะด้วยเลขอ้างอิงได้ตลอดเวลา

### 1.1 URL

ระบบจะเข้าถึงได้จาก `https://www.railway.co.th/complaint/` โดยวางเป็น IIS Application ภายใต้เว็บไซต์หลักของ รฟท. ที่ Deploy บน GDCC

### 1.2 กลุ่มผู้ใช้งาน

| กลุ่ม | การเข้าถึง | หน้าที่ |
|---|---|---|
| ประชาชนทั่วไป | ไม่ต้อง Login | ยื่นเรื่อง, ติดตามสถานะ |
| เจ้าหน้าที่รับเรื่องทั่วไป | Login เลขพนักงาน 7 หลัก | จัดการเรื่องร้องเรียนทั่วไป |
| เจ้าหน้าที่รับเรื่องทุจริต | Login เลขพนักงาน 7 หลัก | จัดการเรื่องทุจริตเท่านั้น |
| Super Admin | Login Username + Password | บริหารระบบ, จัดการผู้ใช้, ตั้งค่า |

---

## 2. Tech Stack

| ส่วน | เทคโนโลยี | เหตุผล |
|---|---|---|
| Framework | ASP.NET Core 8 | รัน IIS บน GDCC ได้ทันที |
| Frontend Pattern | Razor Pages | Deploy เป็น folder เดียว ไม่ต้อง build แยก |
| UI | Tailwind CSS | ยืดหยุ่น, compile รวมใน build |
| Interactivity | HTMX | ลด JavaScript, form-heavy ใช้ได้ดี |
| Database | SQL Server | ตาม requirement |
| ORM | EF Core 8 + Dapper | EF Core สำหรับ CRUD ทั่วไป, Dapper สำหรับ query ซับซ้อน |
| Authentication | ASP.NET Core Identity (Cookie-based) | สำหรับ Staff Login |
| API Auth | JWT Bearer Token | สำหรับ API route ที่เปิดให้ภายนอกใช้ |
| PDF Generation | QuestPDF | สร้างเอกสารสำหรับแนบ eDOC |
| Email | MailKit | ส่ง Email แจ้งเตือน |
| SMS | HTTP Client → SMS Gateway | แจ้งสถานะผ่าน SMS |

---

## 3. โครงสร้างโปรเจกต์

```
SRT.Complaint/
├── Controllers/                    ← API Controllers (สำหรับภายนอก)
│   ├── Api/
│   │   ├── ComplaintsController.cs
│   │   └── StatusController.cs
├── Data/
│   ├── AppDbContext.cs             ← EF Core DbContext หลัก
│   ├── CorruptionDbContext.cs      ← DbContext แยกสำหรับเรื่องทุจริต
│   └── Migrations/
├── Models/
│   ├── Complaint.cs
│   ├── CorruptionReport.cs
│   ├── StaffUser.cs
│   ├── ComplaintCategory.cs
│   ├── SlaConfig.cs
│   └── AuditLog.cs
├── Services/
│   ├── ComplaintService.cs
│   ├── CorruptionService.cs
│   ├── AssignmentService.cs        ← Queue & Auto-assign logic
│   ├── NotificationService.cs
│   ├── PdfExportService.cs
│   ├── MaskingService.cs           ← Censor ข้อมูลส่วนตัว
│   └── SlaService.cs
├── Pages/
│   ├── Shared/
│   │   ├── _Layout.cshtml          ← Layout หลัก
│   │   └── _StaffLayout.cshtml     ← Layout สำหรับ Staff
│   ├── Public/
│   │   ├── Index.cshtml            ← หน้าแรก (เลือก track)
│   │   ├── Submit.cshtml           ← ยื่นเรื่องทั่วไป
│   │   ├── SubmitCorruption.cshtml ← ยื่นเรื่องทุจริต
│   │   └── Track.cshtml            ← ติดตามสถานะ
│   ├── Staff/
│   │   ├── Login.cshtml
│   │   ├── Dashboard.cshtml        ← Queue กลาง
│   │   ├── Queue.cshtml            ← รายการเรื่องทั้งหมด
│   │   ├── CaseDetail.cshtml       ← รายละเอียดเรื่อง
│   │   └── Workload.cshtml         ← Workload ของทีม
│   ├── Corruption/
│   │   ├── Dashboard.cshtml
│   │   ├── Queue.cshtml
│   │   └── CaseDetail.cshtml
│   └── Admin/
│       ├── Dashboard.cshtml
│       ├── Users.cshtml
│       ├── Categories.cshtml
│       ├── SlaSettings.cshtml
│       ├── Notifications.cshtml
│       ├── AuditLog.cshtml
│       └── Reports.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/htmx.min.js
└── Program.cs
```

---

## 4. Database Schema

### 4.1 Schema แยกตามประเภท

เรื่องทุจริตใช้ **SQL Schema แยก** ชื่อ `corruption` เพื่อป้องกัน query ปะปน และง่ายต่อการ set permission ระดับ SQL Server

```
dbo.*         ← เรื่องทั่วไป, Staff, Config, Audit
corruption.*  ← เรื่องทุจริตทั้งหมด
```

---

### 4.2 ตาราง: dbo.StaffUsers

```sql
CREATE TABLE dbo.StaffUsers (
    Id              INT             PRIMARY KEY IDENTITY,
    EmployeeCode    CHAR(7)         NOT NULL UNIQUE,    -- เลขพนักงาน 7 หลัก
    FullName        NVARCHAR(200)   NOT NULL,
    Email           NVARCHAR(200),
    PasswordHash    NVARCHAR(512)   NOT NULL,
    Role            NVARCHAR(50)    NOT NULL,           -- 'GeneralOfficer' | 'CorruptionOfficer' | 'SuperAdmin'
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    LastLoginAt     DATETIME2
);
```

> **หมายเหตุ:** พนักงาน 1 คน ได้รับได้แค่ 1 Role เท่านั้น ไม่มีการรวม Role

---

### 4.3 ตาราง: dbo.ComplaintCategories

```sql
CREATE TABLE dbo.ComplaintCategories (
    Id              INT             PRIMARY KEY IDENTITY,
    Name            NVARCHAR(100)   NOT NULL,
    DepartmentName  NVARCHAR(200),                      -- แผนกรับผิดชอบ
    DefaultPriority NVARCHAR(20)    NOT NULL DEFAULT 'Normal',
    IsActive        BIT             NOT NULL DEFAULT 1,
    SortOrder       INT             NOT NULL DEFAULT 0
);

-- ข้อมูลเริ่มต้น
INSERT INTO dbo.ComplaintCategories (Name, DepartmentName, DefaultPriority) VALUES
(N'ความตรงต่อเวลา', N'ฝ่ายการเดินรถ', 'Normal'),
(N'บริการบนขบวนรถ', N'ฝ่ายการโดยสาร', 'Normal'),
(N'พนักงาน / มารยาท', N'ฝ่ายบริหารทรัพยากรบุคคล', 'Normal'),
(N'สิ่งอำนวยความสะดวก', N'ฝ่ายโยธา', 'Normal'),
(N'ความสะอาด', N'ฝ่ายบริการสถานี', 'Normal'),
(N'ตั๋ว / การคืนเงิน', N'ฝ่ายการพาณิชย์', 'High'),
(N'ความปลอดภัย', N'ฝ่ายรักษาความปลอดภัย', 'Urgent'),
(N'สถานี / ที่จอดรถ', N'ฝ่ายบริการสถานี', 'Normal'),
(N'อื่น ๆ', NULL, 'Normal');
```

---

### 4.4 ตาราง: dbo.SlaConfigs

```sql
CREATE TABLE dbo.SlaConfigs (
    Id                      INT             PRIMARY KEY IDENTITY,
    Priority                NVARCHAR(20)    NOT NULL UNIQUE,  -- 'Critical' | 'Urgent' | 'High' | 'Normal' | 'Low'
    LabelTh                 NVARCHAR(100)   NOT NULL,
    ResolutionHours         INT             NOT NULL,         -- ชั่วโมงที่ต้องปิดเรื่อง
    AutoAssignAfterHours    INT             NOT NULL,         -- ชั่วโมงที่ auto-assign ถ้าไม่มีใครรับ
    WarningThresholdPercent INT             NOT NULL DEFAULT 80, -- แจ้งเตือนเมื่อเหลือ X%
    UpdatedAt               DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedBy               INT             REFERENCES dbo.StaffUsers(Id)
);

-- ค่า Default
INSERT INTO dbo.SlaConfigs (Priority, LabelTh, ResolutionHours, AutoAssignAfterHours) VALUES
('Critical', N'เร่งด่วนมาก (ความปลอดภัย)', 24,  1),
('Urgent',   N'เร่งด่วน',                   72,  4),
('High',     N'สำคัญ',                      120, 8),
('Normal',   N'ปกติ',                       168, 12),
('Low',      N'ข้อเสนอแนะ',                360, 24);
```

---

### 4.5 ตาราง: dbo.Complaints (เรื่องทั่วไป)

```sql
CREATE TABLE dbo.Complaints (
    Id                  INT             PRIMARY KEY IDENTITY,
    ReferenceNumber     NVARCHAR(20)    NOT NULL UNIQUE,  -- GEN-2568-00001
    
    -- ข้อมูลผู้ร้อง
    ReporterName        NVARCHAR(200)   NOT NULL,
    ReporterPhone       NVARCHAR(20)    NOT NULL,
    ReporterEmail       NVARCHAR(200),
    
    -- รายละเอียดเรื่อง
    CategoryId          INT             NOT NULL REFERENCES dbo.ComplaintCategories(Id),
    SubjectStation      NVARCHAR(200),                    -- สถานีที่เกิดเหตุ
    IncidentDate        DATE,
    Description         NVARCHAR(MAX)   NOT NULL,
    
    -- การจัดการ
    Priority            NVARCHAR(20)    NOT NULL DEFAULT 'Normal',
    Status              NVARCHAR(50)    NOT NULL DEFAULT 'Pending',
    AssignedToId        INT             REFERENCES dbo.StaffUsers(Id),
    AssignedAt          DATETIME2,
    
    -- SLA
    SlaDeadline         DATETIME2,                        -- คำนวณจาก Priority + CreatedAt
    SlaBreached         BIT             NOT NULL DEFAULT 0,
    
    -- Timestamps
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETDATE(),
    ClosedAt            DATETIME2,
    
    -- ผลประเมิน
    SatisfactionScore   TINYINT,                          -- 1-5
    SatisfactionNote    NVARCHAR(500)
);

-- Status values:
-- 'Pending'         รอรับเรื่อง
-- 'InProgress'      กำลังดำเนินการ
-- 'WaitingInfo'     รอข้อมูลเพิ่มเติม
-- 'Forwarded'       ส่งต่อแผนก
-- 'Resolved'        แจ้งผลแล้ว
-- 'Closed'          ปิดเรื่อง
-- 'Rejected'        ปฏิเสธ / นอกขอบเขต
```

---

### 4.6 ตาราง: dbo.ComplaintAttachments

```sql
CREATE TABLE dbo.ComplaintAttachments (
    Id              INT             PRIMARY KEY IDENTITY,
    ComplaintId     INT             NOT NULL REFERENCES dbo.Complaints(Id),
    FileName        NVARCHAR(500)   NOT NULL,
    StoredPath      NVARCHAR(1000)  NOT NULL,
    FileSize        BIGINT,
    MimeType        NVARCHAR(100),
    UploadedAt      DATETIME2       NOT NULL DEFAULT GETDATE()
);
```

---

### 4.7 ตาราง: dbo.ComplaintNotes (Internal Notes)

```sql
CREATE TABLE dbo.ComplaintNotes (
    Id              INT             PRIMARY KEY IDENTITY,
    ComplaintId     INT             NOT NULL REFERENCES dbo.Complaints(Id),
    AuthorId        INT             NOT NULL REFERENCES dbo.StaffUsers(Id),
    NoteType        NVARCHAR(20)    NOT NULL,  -- 'Internal' | 'PublicReply' | 'Transfer'
    Content         NVARCHAR(MAX)   NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE()
);
```

---

### 4.8 ตาราง: dbo.ComplaintTransferLog

```sql
CREATE TABLE dbo.ComplaintTransferLog (
    Id              INT             PRIMARY KEY IDENTITY,
    ComplaintId     INT             NOT NULL REFERENCES dbo.Complaints(Id),
    FromOfficerId   INT             REFERENCES dbo.StaffUsers(Id),
    ToOfficerId     INT             NOT NULL REFERENCES dbo.StaffUsers(Id),
    Reason          NVARCHAR(500),
    TransferredAt   DATETIME2       NOT NULL DEFAULT GETDATE(),
    IsAutoAssign    BIT             NOT NULL DEFAULT 0
);
```

---

### 4.9 ตาราง: corruption.Reports (เรื่องทุจริต — Schema แยก)

```sql
CREATE TABLE corruption.Reports (
    Id                      INT             PRIMARY KEY IDENTITY,
    ReferenceNumber         NVARCHAR(20)    NOT NULL UNIQUE,  -- COR-2568-00001
    
    -- ข้อมูลผู้แจ้ง (เข้ารหัส)
    ReporterNameEncrypted   VARBINARY(MAX)  NOT NULL,
    ReporterPhoneEncrypted  VARBINARY(MAX)  NOT NULL,
    ReporterEmailEncrypted  VARBINARY(MAX),
    ReporterIdCardEncrypted VARBINARY(MAX)  NOT NULL,  -- เลขบัตรประชาชน
    
    -- ข้อมูลที่แสดงได้ (Masked)
    ReporterNameMasked      NVARCHAR(200)   NOT NULL,  -- นาย ส***น ก***ย
    ReporterPhoneMasked     NVARCHAR(20)    NOT NULL,  -- 08x-xxx-x456
    ReporterEmailMasked     NVARCHAR(200),
    
    -- รายละเอียดเรื่อง
    SubjectType             NVARCHAR(100)   NOT NULL,  -- ประเภทการทุจริต
    SubjectPersonName       NVARCHAR(200),             -- ชื่อผู้ถูกร้อง (ถ้ามี)
    SubjectDepartment       NVARCHAR(200),
    IncidentDate            DATE,
    Description             NVARCHAR(MAX)   NOT NULL,
    
    -- การจัดการ
    Priority                NVARCHAR(20)    NOT NULL DEFAULT 'Normal',
    Status                  NVARCHAR(50)    NOT NULL DEFAULT 'Pending',
    AssignedToId            INT             REFERENCES dbo.StaffUsers(Id),
    AssignedAt              DATETIME2,
    
    -- SLA
    SlaDeadline             DATETIME2,
    SlaBreached             BIT             NOT NULL DEFAULT 0,
    
    -- Timestamps
    CreatedAt               DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt               DATETIME2       NOT NULL DEFAULT GETDATE(),
    ClosedAt                DATETIME2
);
```

---

### 4.10 ตาราง: corruption.InvestigationLogs (บันทึกการสืบสวน — ลับ)

```sql
CREATE TABLE corruption.InvestigationLogs (
    Id              INT             PRIMARY KEY IDENTITY,
    ReportId        INT             NOT NULL REFERENCES corruption.Reports(Id),
    AuthorId        INT             NOT NULL REFERENCES dbo.StaffUsers(Id),
    Content         NVARCHAR(MAX)   NOT NULL,
    IsConfidential  BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE()
);
```

---

### 4.11 ตาราง: corruption.DecryptionLogs (บันทึกการ Decrypt)

```sql
CREATE TABLE corruption.DecryptionLogs (
    Id              INT             PRIMARY KEY IDENTITY,
    ReportId        INT             NOT NULL REFERENCES corruption.Reports(Id),
    RequestedById   INT             NOT NULL REFERENCES dbo.StaffUsers(Id),
    Reason          NVARCHAR(500)   NOT NULL,
    RequestedAt     DATETIME2       NOT NULL DEFAULT GETDATE(),
    IpAddress       NVARCHAR(50)
);
```

---

### 4.12 ตาราง: dbo.AuditLogs

```sql
CREATE TABLE dbo.AuditLogs (
    Id              BIGINT          PRIMARY KEY IDENTITY,
    ActorId         INT             REFERENCES dbo.StaffUsers(Id),
    ActorCode       NVARCHAR(20),   -- เก็บ EmployeeCode ไว้แม้ลบ User แล้ว
    Action          NVARCHAR(100)   NOT NULL,  -- 'Login' | 'ClaimCase' | 'UpdateStatus' | ...
    EntityType      NVARCHAR(50),              -- 'Complaint' | 'CorruptionReport' | 'StaffUser'
    EntityId        NVARCHAR(50),
    Detail          NVARCHAR(MAX),             -- JSON detail
    IpAddress       NVARCHAR(50),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE()
);
```

---

### 4.13 ตาราง: dbo.NotificationTemplates

```sql
CREATE TABLE dbo.NotificationTemplates (
    Id              INT             PRIMARY KEY IDENTITY,
    EventKey        NVARCHAR(100)   NOT NULL UNIQUE,  -- 'ComplaintReceived' | 'StatusChanged' | ...
    LabelTh         NVARCHAR(200)   NOT NULL,
    EmailSubject    NVARCHAR(300),
    EmailBody       NVARCHAR(MAX),
    SmsBody         NVARCHAR(500),
    IsEmailEnabled  BIT             NOT NULL DEFAULT 1,
    IsSmsEnabled    BIT             NOT NULL DEFAULT 1
);
```

---

## 5. Authentication & Authorization

### 5.1 Staff Login Flow

```
POST /staff/login
  → รับ EmployeeCode (7 หลัก) + Password
  → ตรวจ dbo.StaffUsers
  → ถ้าถูก → สร้าง Cookie Session (ASP.NET Core Identity)
  → บันทึก AuditLog (Action: 'Login')
  → Redirect ตาม Role:
      GeneralOfficer    → /staff/dashboard
      CorruptionOfficer → /corruption/dashboard
      SuperAdmin        → /admin/dashboard
```

### 5.2 Authorization Policies

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GeneralOfficer", policy =>
        policy.RequireRole("GeneralOfficer"));

    options.AddPolicy("CorruptionOfficer", policy =>
        policy.RequireRole("CorruptionOfficer"));

    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole("GeneralOfficer", "CorruptionOfficer", "SuperAdmin"));

    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));
});
```

### 5.3 API JWT (สำหรับภายนอก)

- API Routes ทั้งหมดใต้ `/api/` ใช้ JWT Bearer Token
- Super Admin สร้าง API Key ให้ระบบภายนอกได้จากหน้า Settings
- Token มีอายุ 1 ปี (ปรับได้)

---

## 6. หน้าเว็บ — Public

### 6.1 หน้าแรก `/complaint/`

แสดง 2 ตัวเลือก พร้อมอธิบายความแตกต่าง:

- **ยื่นเรื่องร้องเรียนทั่วไป** → `/complaint/submit`
- **แจ้งเบาะแสทุจริต / พฤติกรรมมิชอบ** → `/complaint/submit-corruption`

มีช่อง **ติดตามสถานะ** (กรอกเลขอ้างอิง) อยู่ที่หน้าแรกด้วย

---

### 6.2 ยื่นเรื่องทั่วไป `/complaint/submit`

**ฟิลด์ที่จำเป็น (required):**
- ชื่อ-นามสกุล
- เบอร์โทรศัพท์
- ประเภทเรื่อง (Dropdown จาก ComplaintCategories)
- รายละเอียดเรื่องร้องเรียน

**ฟิลด์เสริม (optional):**
- อีเมล
- สถานีหรือพื้นที่ที่เกิดเหตุ
- วัน/เวลาที่เกิดเหตุ
- ไฟล์แนบ (รูปภาพ / เอกสาร — ไม่เกิน 10MB ต่อไฟล์, สูงสุด 5 ไฟล์)

**หลังกด Submit:**
1. ระบบออกเลขอ้างอิง รูปแบบ `GEN-2568-XXXXX`
2. คำนวณ SLA Deadline จาก Priority ของ Category
3. ส่ง SMS และ/หรือ Email พร้อมเลขอ้างอิงและลิงก์ติดตาม
4. บันทึก AuditLog

---

### 6.3 ยื่นเรื่องทุจริต `/complaint/submit-corruption`

**แจ้งผู้ใช้ชัดเจน:** ข้อมูลตัวตนจะถูกเก็บเป็นความลับ เจ้าหน้าที่จะเห็นเฉพาะชื่อที่ถูกปิดบัง

**ฟิลด์ที่จำเป็น (required):**
- ชื่อ-นามสกุล *(จัดเก็บแบบ Encrypted)*
- เลขบัตรประชาชน *(จัดเก็บแบบ Encrypted — ใช้ยืนยันตัวตน ป้องกันบัตรสนเท่ห์)*
- เบอร์โทรศัพท์ *(จัดเก็บแบบ Encrypted)*
- ประเภทการทุจริต (Dropdown)
- รายละเอียดเบาะแส

**ฟิลด์เสริม:**
- อีเมล
- ชื่อ/แผนกผู้ถูกร้อง
- วัน/เวลาที่เกิดเหตุ
- ไฟล์แนบ

**Encryption:** ใช้ AES-256 โดย Key เก็บใน Environment Variable บน Server ไม่ได้เก็บใน DB

**หลัง Submit:** ออกเลขอ้างอิง `COR-2568-XXXXX` พร้อมส่ง SMS/Email

---

### 6.4 ติดตามสถานะ `/complaint/track`

- กรอกเลขอ้างอิง (GEN-XXXX หรือ COR-XXXX)
- ระบบแสดง Timeline ความคืบหน้า (วัน/เวลา + สถานะ)
- แสดงเฉพาะข้อมูลที่เจ้าหน้าที่อนุญาตให้เปิดเผย
- ถ้าเรื่องปิดแล้วและมีการประเมินความพึงพอใจ จะแสดง Rating Form

---

## 7. หน้าเว็บ — เจ้าหน้าที่รับเรื่องทั่วไป

### 7.1 Dashboard `/staff/dashboard`

แสดง:
- **Inbox (คิวกลาง):** เรื่องที่ยังไม่มีใครรับ เรียงตาม Priority + วันที่
- **My Cases:** เรื่องที่ตัวเองรับแล้ว แยกตามสถานะ
- **SLA Warning:** เรื่องที่ใกล้ครบกำหนด (ไฮไลต์สีแดง/เหลือง)
- **Workload ทีม:** จำนวน Open Case ของแต่ละคนในทีม (ตัวเลขเท่านั้น ไม่เปิดเผยรายละเอียดเรื่อง)

### 7.2 รายการเรื่อง `/staff/queue`

ตาราง Filter ได้ตาม:
- สถานะ
- ประเภทเรื่อง
- Priority
- วันที่รับ
- ชื่อเจ้าหน้าที่

### 7.3 รายละเอียดเรื่อง `/staff/case/{id}`

**ข้อมูลที่แสดง:**
- ข้อมูลผู้ร้องเรียน
- รายละเอียดเรื่อง + ไฟล์แนบ
- Timeline ทุก Action
- Note ภายใน (Internal Notes)
- SLA Countdown

**Action ที่ทำได้:**
- **รับเรื่อง** (Claim from Pool)
- **อัปเดตสถานะ**
- **เพิ่ม Note** (Internal หรือ ตอบกลับผู้ร้อง)
- **โอนเรื่อง** ให้เพื่อนร่วมงาน (ต้องระบุเหตุผล)
- **ส่งต่อแผนก** (บันทึกชื่อแผนกที่ส่งไป)
- **ปฏิเสธเรื่อง** (ต้องระบุเหตุผล)
- **ปิดเรื่อง** (แจ้งผลสรุปให้ผู้ร้อง)
- **สร้างเอกสาร eDOC** (Download PDF)

---

## 8. หน้าเว็บ — เจ้าหน้าที่รับเรื่องทุจริต

โครงสร้างเหมือนเจ้าหน้าที่ทั่วไป แต่:

- URL อยู่ใต้ `/corruption/`
- ข้อมูลผู้แจ้งแสดงเป็นเวอร์ชัน Masked เสมอ
- มีปุ่ม **"ขอดูข้อมูลจริง"** → ต้องกรอกเหตุผล → บันทึก DecryptionLog ทุกครั้ง
- บันทึกการสืบสวนอยู่ใน Tab แยก (Confidential)
- ไม่มีปุ่มประเมินความพึงพอใจ
- **สร้างเอกสาร eDOC** → PDF ใช้ชื่อ Masked โดยอัตโนมัติ

---

## 9. หน้าเว็บ — Super Admin

### 9.1 Dashboard `/admin/dashboard`

- จำนวนเรื่องรับวันนี้ / สัปดาห์นี้ (แยก Track)
- อัตราการปิดเรื่องภายใน SLA
- เรื่องที่เกิน SLA
- กราฟแนวโน้มรายเดือน
- **เรื่องทุจริต: แสดงเฉพาะตัวเลขสถิติ ไม่แสดงรายละเอียด**

### 9.2 จัดการผู้ใช้ `/admin/users`

- รายชื่อเจ้าหน้าที่ทั้งหมด
- เพิ่มเจ้าหน้าที่ใหม่ (กรอกเลขพนักงาน + กำหนด Role)
- แก้ไข Role
- ระงับ / เปิดใช้งานบัญชี
- Reset Password

### 9.3 ตั้งค่า SLA `/admin/sla`

- แสดงตาราง SLA ทุก Priority
- แก้ไข ResolutionHours, AutoAssignAfterHours, WarningThresholdPercent ได้เลย
- บันทึกแล้วมีผลกับเรื่องใหม่ทันที (เรื่องเก่าใช้ SLA ตอนที่รับเรื่อง)

### 9.4 จัดการหมวดหมู่ `/admin/categories`

- CRUD หมวดหมู่
- กำหนดแผนกรับผิดชอบ
- กำหนด Default Priority
- ซ่อน/แสดงหมวดหมู่

### 9.5 การแจ้งเตือน `/admin/notifications`

- เปิด/ปิด SMS และ Email แยกต่ามกันแต่ละ Event
- แก้ไข Template ข้อความ (มี placeholder เช่น `{ReferenceNumber}`, `{Status}`)
- กำหนด Email ผู้รับแจ้งเตือนเร่งด่วน (กรณีเรื่อง Critical)

### 9.6 Audit Log `/admin/audit`

- ตารางแสดง Log ทุก Action
- Filter ตาม: เจ้าหน้าที่, Action, วันที่, Entity
- Export เป็น Excel

### 9.7 รายงาน `/admin/reports`

- Export รายงานสรุปรายเดือน/รายไตรมาส เป็น Excel หรือ PDF
- สถิติตามหมวดหมู่, ตามสถานี, ตามเจ้าหน้าที่
- อัตราความพึงพอใจเฉลี่ย

---

## 10. Queue & Assignment System

### 10.1 หลักการ: Shared Pool + Self-Claim + Auto-assign Fallback

```
เรื่องใหม่เข้า
      ↓
วางใน Shared Pool (Status = 'Pending')
      ↓
เจ้าหน้าที่เห็นคิวกลาง → กดรับเรื่อง (Self-Claim)
      ↓
ถ้าไม่มีใครรับภายใน AutoAssignAfterHours (ตาม SLA Config)
      ↓
Background Job คำนวณ: เจ้าหน้าที่คนไหน Open Case น้อยที่สุด
      ↓
Auto-assign + ส่ง Notification ให้เจ้าหน้าที่คนนั้น
```

### 10.2 Transfer Rules

- เจ้าหน้าที่โอนเรื่องให้เพื่อนร่วมงานได้ — ต้องระบุเหตุผล
- Super Admin บังคับ Reassign ได้ทุกเวลา
- ทุกการโอนบันทึกใน ComplaintTransferLog

### 10.3 Background Job (SlaService)

รัน Hosted Service ทุก 30 นาที:
1. ตรวจเรื่องที่ AutoAssignAfterHours ครบแล้วยังไม่มีคนรับ → Auto-assign
2. ตรวจเรื่องที่ใกล้ครบ SLA (ถึง WarningThreshold) → ส่ง Notification เตือน
3. ตรวจเรื่องที่เกิน SLA Deadline → Mark `SlaBreached = true` + แจ้ง Super Admin

---

## 11. eDOC Export

### 11.1 PDF Template

ปุ่ม "สร้างเอกสารสำหรับ eDOC" อยู่ในหน้า Case Detail เนื้อหาใน PDF:

```
หัวจดหมาย: การรถไฟแห่งประเทศไทย
            บันทึกเรื่องร้องเรียน

เลขที่คำร้อง:     GEN-2568-00142
วันที่รับเรื่อง:   15 มิถุนายน 2568
ประเภทเรื่อง:      ความตรงต่อเวลา
ความเร่งด่วน:      ปกติ
กำหนดส่งผล:       22 มิถุนายน 2568

ข้อมูลผู้ร้องเรียน:
  ชื่อ-สกุล:       นายสมชาย ใจดี
  เบอร์โทร:        089-123-4567
  อีเมล:           somchai@email.com

สถานที่/สถานี:    สถานีกลางกรุงเทพอภิวัฒน์
วัน/เวลาเกิดเหตุ:  14 มิถุนายน 2568

รายละเอียดคำร้อง:
  [รายละเอียดเต็ม]

เจ้าหน้าที่รับเรื่อง:  นายสมศักดิ์ มั่นคง (1234567)
วันที่พิมพ์:           15 มิถุนายน 2568 เวลา 10:30 น.

หมายเหตุภายใน: [ถ้ามี]
```

> เรื่องทุจริต: ชื่อผู้แจ้งในเอกสาร PDF จะเป็นเวอร์ชัน Masked โดยอัตโนมัติ

### 11.2 Future API Endpoint (เตรียมไว้)

```
GET /api/complaints/{referenceNumber}/edoc-payload
Authorization: Bearer {token}

Response: {
  "referenceNumber": "GEN-2568-00142",
  "reporterName": "นายสมชาย ใจดี",
  "category": "ความตรงต่อเวลา",
  "description": "...",
  "assignedOfficer": "...",
  "createdAt": "2025-06-15T10:00:00",
  "slaDeadline": "2025-06-22T10:00:00",
  "status": "InProgress"
}
```

---

## 12. Notification System

### 12.1 Events และ Template

| Event Key | ผู้รับ | SMS | Email |
|---|---|---|---|
| `ComplaintReceived` | ผู้ร้อง | ✓ | ✓ |
| `StatusChanged` | ผู้ร้อง | ✓ | ✓ |
| `ComplaintClosed` | ผู้ร้อง | ✓ | ✓ |
| `AutoAssigned` | เจ้าหน้าที่ | - | ✓ |
| `SlaWarning` | เจ้าหน้าที่ + Super Admin | - | ✓ |
| `SlaBreached` | Super Admin | - | ✓ |
| `CriticalReceived` | Super Admin | - | ✓ |
| `DecryptionRequested` | Super Admin | - | ✓ |

### 12.2 SMS Template ตัวอย่าง

```
[รฟท.] รับเรื่องร้องเรียนของท่านแล้ว เลขที่ {ReferenceNumber}
ติดตามสถานะ: https://www.railway.co.th/complaint/track/{ReferenceNumber}
```

---

## 13. API Layer (สำหรับระบบภายนอก)

เปิด endpoint ไว้รอ เชื่อมต่อได้เมื่อพร้อม:

```
POST   /api/complaints                    ← ยื่นเรื่องจากระบบภายนอก
GET    /api/complaints/{referenceNumber}  ← ดูสถานะ
GET    /api/complaints/{referenceNumber}/edoc-payload ← ดึง payload สำหรับ eDOC
GET    /api/stats/summary                 ← สถิติรวม (ต้องมี token)
```

ทุก endpoint ต้องใช้ JWT Bearer Token ยกเว้น `/api/complaints` ที่ public เข้าได้

---

## 14. Security

- **Password:** Hashed ด้วย BCrypt (cost factor 12)
- **Encryption:** AES-256-CBC สำหรับข้อมูลผู้แจ้งทุจริต, Key อยู่ใน Environment Variable
- **Session:** HttpOnly Cookie, Secure flag, SameSite=Strict
- **CSRF:** ASP.NET Core Anti-forgery Token ทุก Form
- **File Upload:** ตรวจ MIME Type, จำกัด Extension (.jpg .png .pdf .doc .docx), Scan ชื่อไฟล์
- **Input:** Parameterized Query ทุก Query, Razor encode HTML โดย default
- **Rate Limiting:** จำกัด Submit 5 ครั้ง / IP / ชั่วโมง
- **HTTPS:** บังคับ HTTPS ทุก Request

---

## 15. Deployment

### 15.1 โครงสร้างบน IIS (GDCC)

```
IIS Site: railway.co.th
└── Application: /complaint
    ├── Application Pool: .NET 8 (No Managed Code)
    ├── Deploy path: C:\inetpub\wwwroot\complaint\
    └── Environment: Production
```

### 15.2 Environment Variables (ตั้งใน IIS หรือ appsettings.Production.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=SRT_Complaint;..."
  },
  "Encryption": {
    "Key": "...(256-bit base64)..."
  },
  "Jwt": {
    "Secret": "...",
    "Issuer": "railway.co.th",
    "ExpiryDays": 365
  },
  "Notifications": {
    "SmsGatewayUrl": "...",
    "SmsApiKey": "...",
    "SmtpHost": "...",
    "SmtpPort": 587,
    "SmtpUser": "...",
    "SmtpPassword": "..."
  },
  "FileUpload": {
    "StoragePath": "C:\\SRT_Uploads\\Complaints\\"
  }
}
```

### 15.3 ขั้นตอน Deploy

1. `dotnet publish -c Release -o ./publish`
2. Copy `./publish` ไปวางที่ `C:\inetpub\wwwroot\complaint\`
3. ตั้งค่า Environment Variables บน IIS Application Pool
4. รัน EF Core Migration: `dotnet ef database update`
5. ตรวจสอบ `https://www.railway.co.th/complaint/`

---

## 16. สรุปสิ่งที่ต้องทำก่อน Development

- [ ] ขอ Connection String SQL Server จาก GDCC
- [ ] ขอ SMTP credentials สำหรับส่ง Email ของ รฟท.
- [ ] ขอ SMS Gateway API จากผู้ให้บริการ
- [ ] กำหนด AES Encryption Key และเก็บอย่างปลอดภัย
- [ ] ยืนยัน Path ไฟล์แนบ (ต้องเป็น Drive ที่ IIS เขียนได้)
- [ ] ยืนยันว่า IIS รัน .NET 8 Hosting Bundle แล้ว
- [ ] ยืนยัน URL สุดท้าย (`/complaint` หรืออื่น)

---

## 17. API Management

### 17.1 ภาพรวม

ระบบมี API Management Layer ในตัว ให้ Super Admin สร้างและบริหาร API Key ได้จากหน้า Admin โดยไม่ต้องแตะ code รองรับทั้ง Internal (ระบบของ รฟท. ด้วยกัน) และ External (หน่วยงานภายนอกที่ได้รับอนุญาต)

**ผู้ใช้ API ที่คาดหวัง:**

| ประเภท | ตัวอย่าง | หมายเหตุ |
|---|---|---|
| Internal | ระบบ eDOC, Mobile App รฟท. | เข้าถึง Scope ได้มากกว่า |
| External | หน่วยงานภาครัฐ, ผู้พัฒนาที่ได้รับอนุญาต | จำกัด Scope เฉพาะที่จำเป็น |

---

### 17.2 Database Schema — API Management

#### ตาราง: dbo.ApiKeys

```sql
CREATE TABLE dbo.ApiKeys (
    Id              INT             PRIMARY KEY IDENTITY,
    Name            NVARCHAR(200)   NOT NULL,               -- ชื่อ Consumer เช่น "ระบบ eDOC รฟท."
    KeyType         NVARCHAR(20)    NOT NULL,               -- 'Internal' | 'External'
    KeyPrefix       NVARCHAR(20)    NOT NULL,               -- 8 ตัวแรกของ Key สำหรับแสดงใน UI
    KeyHash         NVARCHAR(512)   NOT NULL,               -- SHA-256 Hash ของ Key จริง
    RateLimitPerMin INT             NOT NULL DEFAULT 60,
    AllowedIps      NVARCHAR(MAX),                          -- JSON Array เช่น ["10.0.0.0/24"] หรือ NULL = ไม่จำกัด
    ExpiresAt       DATETIME2,                              -- NULL = ไม่มีวันหมดอายุ
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedById     INT             NOT NULL REFERENCES dbo.StaffUsers(Id),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    LastUsedAt      DATETIME2,
    RevokedAt       DATETIME2,
    RevokedById     INT             REFERENCES dbo.StaffUsers(Id),
    RevokedReason   NVARCHAR(500)
);
```

> **หมายเหตุ:** Key จริงไม่ถูกเก็บใน DB เลย — เก็บเฉพาะ Hash ดังนั้นถ้า Key หาย ต้องสร้างใหม่และยกเลิกอันเดิมเท่านั้น

---

#### ตาราง: dbo.ApiKeyScopes

```sql
CREATE TABLE dbo.ApiKeyScopes (
    Id          INT             PRIMARY KEY IDENTITY,
    ApiKeyId    INT             NOT NULL REFERENCES dbo.ApiKeys(Id) ON DELETE CASCADE,
    Scope       NVARCHAR(100)   NOT NULL
);

-- Index สำหรับตรวจ Scope ต่อ Key อย่างรวดเร็ว
CREATE INDEX IX_ApiKeyScopes_ApiKeyId ON dbo.ApiKeyScopes(ApiKeyId);
```

---

#### ตาราง: dbo.ApiRequestLogs

```sql
CREATE TABLE dbo.ApiRequestLogs (
    Id              BIGINT          PRIMARY KEY IDENTITY,
    ApiKeyId        INT             NOT NULL REFERENCES dbo.ApiKeys(Id),
    HttpMethod      NVARCHAR(10)    NOT NULL,               -- GET | POST | PUT | DELETE
    Endpoint        NVARCHAR(500)   NOT NULL,
    QueryString     NVARCHAR(1000),
    IpAddress       NVARCHAR(50),
    ResponseStatus  INT             NOT NULL,               -- 200, 201, 401, 403, 429...
    ResponseMs      INT,                                    -- เวลาตอบสนอง (milliseconds)
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE()
);

-- Partition หรือ Index ตาม CreatedAt สำหรับ query ตามช่วงเวลา
CREATE INDEX IX_ApiRequestLogs_ApiKeyId_CreatedAt
    ON dbo.ApiRequestLogs(ApiKeyId, CreatedAt DESC);
```

> **หมายเหตุ:** Log เก็บทุก Request — ทั้งที่สำเร็จและล้มเหลว กำหนด Retention Policy เก็บ 90 วัน แล้วลบอัตโนมัติ

---

#### ตาราง: dbo.Webhooks

```sql
CREATE TABLE dbo.Webhooks (
    Id              INT             PRIMARY KEY IDENTITY,
    ApiKeyId        INT             NOT NULL REFERENCES dbo.ApiKeys(Id),
    Name            NVARCHAR(200)   NOT NULL,               -- ชื่อ Webhook เพื่อความเข้าใจ
    TargetUrl       NVARCHAR(1000)  NOT NULL,               -- URL ปลายทาง
    SecretHash      NVARCHAR(512)   NOT NULL,               -- HMAC Secret สำหรับ X-SRT-Signature
    Events          NVARCHAR(MAX)   NOT NULL,               -- JSON Array ของ Event ที่ Subscribe
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    LastTriggeredAt DATETIME2,
    LastStatusCode  INT                                     -- HTTP Status ที่ได้รับจากปลายทางล่าสุด
);

-- Events ที่รองรับ (เก็บใน JSON Array):
-- 'complaint.created'         เรื่องใหม่ถูกยื่น
-- 'complaint.status_changed'  สถานะเรื่องเปลี่ยน
-- 'complaint.closed'          เรื่องถูกปิด
-- 'complaint.sla_breached'    เรื่องเกิน SLA
```

---

#### ตาราง: dbo.WebhookDeliveryLogs

```sql
CREATE TABLE dbo.WebhookDeliveryLogs (
    Id              BIGINT          PRIMARY KEY IDENTITY,
    WebhookId       INT             NOT NULL REFERENCES dbo.Webhooks(Id),
    EventType       NVARCHAR(100)   NOT NULL,
    Payload         NVARCHAR(MAX)   NOT NULL,               -- JSON ที่ส่งไป
    AttemptCount    TINYINT         NOT NULL DEFAULT 1,
    ResponseStatus  INT,                                    -- HTTP Status จากปลายทาง
    ResponseBody    NVARCHAR(MAX),
    IsDelivered     BIT             NOT NULL DEFAULT 0,
    NextRetryAt     DATETIME2,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    DeliveredAt     DATETIME2
);
```

---

### 17.3 Scopes ทั้งหมด

| Scope | คำอธิบาย | Internal | External |
|---|---|---|---|
| `complaints:read` | ดูรายละเอียดเรื่องร้องเรียนทั่วไป | ✓ | ✓ |
| `complaints:write` | ยื่นเรื่องใหม่ผ่าน API | ✓ | ✓ |
| `complaints:status` | ตรวจสอบสถานะด้วยเลขอ้างอิง | ✓ | ✓ |
| `complaints:update` | อัปเดตสถานะเรื่องผ่าน API | ✓ | — |
| `complaints:edoc` | ดึง Payload สำหรับส่งต่อ eDOC | ✓ | — |
| `stats:summary` | สถิติรวม (จำนวน, อัตราปิด, SLA) | ✓ | ✓ |
| `stats:detailed` | สถิติรายละเอียด แยกประเภท/แผนก | ✓ | — |
| `corruption:stats` | สถิติเรื่องทุจริต (ตัวเลขเท่านั้น ไม่มีรายละเอียด) | ✓ | — |
| `webhooks:manage` | ลงทะเบียนและจัดการ Webhook URL | ✓ | ✓ |

> **หมายเหตุ:** Scope ที่ไม่เปิดสำหรับ External จะไม่ปรากฏในหน้าสร้าง Key ประเภท External เลย

---

### 17.4 Request Validation Pipeline

ทุก Request ที่เข้ามาผ่าน `/api/` จะถูกตรวจตามลำดับนี้ก่อนถึง Controller:

```
1. อ่าน Key จาก Header: X-API-Key หรือ Authorization: Bearer
       ↓ ไม่มี Header → 401 Unauthorized
2. Hash Key แล้วเทียบกับ dbo.ApiKeys
       ↓ ไม่ตรง → 401 Unauthorized
3. ตรวจ IsActive และ ExpiresAt
       ↓ ถูกยกเลิก หรือหมดอายุ → 401 Unauthorized
4. ตรวจ IP Whitelist (ถ้า AllowedIps ไม่ใช่ NULL)
       ↓ IP ไม่อยู่ใน Whitelist → 403 Forbidden + บันทึก Alert
5. ตรวจ Rate Limit (Sliding Window 1 นาที ใน Memory Cache)
       ↓ เกิน Limit → 429 Too Many Requests + Header: Retry-After
6. ตรวจ Scope ตาม Endpoint ที่เรียก
       ↓ Scope ไม่พอ → 403 Forbidden
7. ส่งต่อไปยัง Controller → ประมวลผล → Response
       ↓ บันทึก ApiRequestLog ทุกกรณี (รวม Error)
```

---

### 17.5 API Endpoints

```
# เรื่องร้องเรียนทั่วไป
POST   /api/complaints                              (complaints:write)
GET    /api/complaints/{referenceNumber}            (complaints:read)
GET    /api/complaints/{referenceNumber}/status     (complaints:status)
PUT    /api/complaints/{referenceNumber}/status     (complaints:update)
GET    /api/complaints/{referenceNumber}/edoc-payload (complaints:edoc)

# สถิติ
GET    /api/stats/summary                           (stats:summary)
GET    /api/stats/detailed                          (stats:detailed)
GET    /api/stats/corruption                        (corruption:stats)

# Webhook
GET    /api/webhooks                                (webhooks:manage)
POST   /api/webhooks                                (webhooks:manage)
DELETE /api/webhooks/{id}                           (webhooks:manage)
```

---

### 17.6 Request / Response ตัวอย่าง

#### ยื่นเรื่องร้องเรียน

```http
POST /api/complaints
X-API-Key: srt_live_k1a2b3c4...
Content-Type: application/json

{
  "reporterName": "นายสมชาย ใจดี",
  "reporterPhone": "0891234567",
  "reporterEmail": "somchai@email.com",
  "categoryId": 1,
  "subjectStation": "สถานีกลางกรุงเทพอภิวัฒน์",
  "incidentDate": "2025-06-14",
  "description": "รถไฟมาช้ากว่ากำหนด 2 ชั่วโมง..."
}
```

```json
HTTP 201 Created
{
  "referenceNumber": "GEN-2568-00142",
  "status": "Pending",
  "slaDeadline": "2025-06-21T10:00:00",
  "trackingUrl": "https://www.railway.co.th/complaint/track/GEN-2568-00142"
}
```

#### ดึง eDOC Payload

```http
GET /api/complaints/GEN-2568-00142/edoc-payload
X-API-Key: srt_live_k1a2b3c4...
```

```json
HTTP 200 OK
{
  "referenceNumber": "GEN-2568-00142",
  "receivedAt": "2025-06-15T10:00:00",
  "category": "ความตรงต่อเวลา",
  "priority": "Normal",
  "reporterName": "นายสมชาย ใจดี",
  "reporterPhone": "0891234567",
  "subjectStation": "สถานีกลางกรุงเทพอภิวัฒน์",
  "incidentDate": "2025-06-14",
  "description": "รถไฟมาช้ากว่ากำหนด 2 ชั่วโมง...",
  "assignedOfficer": "นายสมศักดิ์ มั่นคง",
  "assignedOfficerCode": "1234567",
  "status": "InProgress",
  "slaDeadline": "2025-06-21T10:00:00"
}
```

---

### 17.7 Webhook Payload ตัวอย่าง

```http
POST https://your-system.example.com/webhook
Content-Type: application/json
X-SRT-Signature: sha256=abc123...   ← HMAC-SHA256 ของ Payload + Secret

{
  "event": "complaint.status_changed",
  "timestamp": "2025-06-15T14:30:00Z",
  "data": {
    "referenceNumber": "GEN-2568-00142",
    "previousStatus": "Pending",
    "newStatus": "InProgress",
    "updatedBy": "1234567"
  }
}
```

**Retry Policy:** ถ้าปลายทางตอบ HTTP ≥ 400 หรือ Timeout ระบบ Retry อัตโนมัติ:
- ครั้งที่ 1: ทันที
- ครั้งที่ 2: หลัง 5 นาที
- ครั้งที่ 3: หลัง 30 นาที
- หลังจากนั้น: Mark `IsDelivered = false` และแจ้ง Super Admin

---

### 17.8 หน้า API Management ใน Admin Panel

**หน้า `/admin/api-keys`**

- ตารางรายการ API Keys ทั้งหมด พร้อม Status, ประเภท, ใช้งานล่าสุด
- ปุ่ม "สร้าง Key ใหม่" → Form: ชื่อ, ประเภท, Scopes (checkbox), Rate Limit, IP Whitelist, วันหมดอายุ
- กด Generate → แสดง Full Key ครั้งเดียว พร้อมปุ่ม Copy
- ปุ่ม "ยกเลิก Key" → ต้องระบุเหตุผล → บันทึก AuditLog

**หน้า `/admin/api-keys/{id}/usage`**

- สถิติ: Request วันนี้, Success Rate, Error Rate, Avg Response Time
- กราฟ Request ต่อชั่วโมง (24 ชั่วโมงล่าสุด)
- ตาราง Request Log ล่าสุด (Method, Endpoint, Status, IP, เวลา)
- Filter ตาม Status Code, วันที่

**หน้า `/admin/webhooks`**

- รายการ Webhooks ที่ลงทะเบียนไว้ พร้อม URL, Events, Status ล่าสุด
- ปุ่ม "Test" → ส่ง Payload ทดสอบไปยัง URL
- ดู Delivery Log ย้อนหลัง

---

### 17.9 Environment Variables เพิ่มเติม

เพิ่มจาก Section 15.2:

```json
{
  "ApiManagement": {
    "WebhookSigningKeyPrefix": "SRT_WHK_",
    "RequestLogRetentionDays": 90,
    "RateLimitWindowSeconds": 60
  }
}
```

---

## 18. สรุปสิ่งที่ต้องทำก่อน Development

- [ ] ขอ Connection String SQL Server จาก GDCC
- [ ] ขอ SMTP credentials สำหรับส่ง Email ของ รฟท.
- [ ] ขอ SMS Gateway API จากผู้ให้บริการ
- [ ] กำหนด AES Encryption Key และเก็บอย่างปลอดภัย
- [ ] ยืนยัน Path ไฟล์แนบ (ต้องเป็น Drive ที่ IIS เขียนได้)
- [ ] ยืนยันว่า IIS รัน .NET 8 Hosting Bundle แล้ว
- [ ] ยืนยัน URL สุดท้าย (`/complaint` หรืออื่น)
- [ ] ตัดสินใจ Retention Policy สำหรับ ApiRequestLogs (แนะนำ 90 วัน)
- [ ] กำหนดว่า External API จะเปิดให้ใครได้บ้าง (ต้องมีกระบวนการขออนุญาต)

---

*จัดทำโดย: ฝ่ายพัฒนาระบบ รฟท. — เอกสารนี้ใช้สำหรับ Development เท่านั้น*
