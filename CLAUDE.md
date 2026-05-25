# ระบบรับเรื่องร้องเรียน การรถไฟแห่งประเทศไทย

## ภาพรวมโปรเจกต์

ระบบรับเรื่องร้องเรียนออนไลน์ของ รฟท. รองรับ 2 Track:
- **เรื่องทั่วไป:** ยื่นเรื่อง ติดตาม และ จัดการโดยเจ้าหน้าที่
- **เรื่องทุจริต:** Track แยก เข้ารหัสข้อมูลผู้แจ้ง ใช้ DB Schema แยก

URL: `https://www.railway.co.th/complaint/`
Deploy: IIS Application บน GDCC

## Tech Stack

- **Framework:** ASP.NET Core 8 (Razor Pages)
- **Frontend:** Tailwind CSS + HTMX
- **Database:** SQL Server (EF Core 8 + Dapper)
- **Auth:** ASP.NET Core Identity (Cookie) + JWT (API)
- **PDF:** QuestPDF
- **Email:** MailKit
- **SMS:** HTTP Client → SMS Gateway

## โครงสร้างโปรเจกต์

```
SRT.Complaint/
├── Controllers/Api/        ← API endpoints สำหรับ External
├── Data/
│   ├── AppDbContext.cs     ← EF Core DbContext หลัก
│   └── CorruptionDbContext.cs ← DbContext แยกสำหรับทุจริต
├── Models/
├── Services/               ← Business logic แยกออกจาก Pages
├── Pages/
│   ├── Public/             ← ยื่นเรื่อง, ติดตาม (ไม่ต้อง Login)
│   ├── Staff/              ← เจ้าหน้าที่ทั่วไป
│   ├── Corruption/         ← เจ้าหน้าที่ทุจริต
│   └── Admin/              ← Super Admin
└── wwwroot/
```

## Database Schema Guideline

- เรื่องทั่วไป: ใช้ Schema `dbo.*`
- เรื่องทุจริต: ใช้ Schema `corruption.*` (แยกสมบูรณ์)
- ข้อมูลผู้แจ้งทุจริต: เข้ารหัส AES-256, เก็บ Masked version แยก

### ตัวอย่างตารางหลัก

- `dbo.StaffUsers` — เจ้าหน้าที่ (Login ด้วยเลขพนักงาน 7 หลัก)
- `dbo.Complaints` — เรื่องร้องเรียนทั่วไป
- `corruption.Reports` — เรื่องทุจริต (Schema แยก)
- `dbo.ApiKeys` — API Management
- `dbo.ApiRequestLogs` — Log ทุก API request

ดูรายละเอียดเต็มใน `srt-complaint-system-spec.md`

## Coding Standards

### C# Style

- **Naming:** PascalCase สำหรับ class/method/property, camelCase สำหรับ local variables
- **Async:** ทุก I/O operation ต้องเป็น async/await
- **Null handling:** ใช้ nullable reference types (`#nullable enable`)
- **Error handling:** ใช้ try-catch ที่ Controller/Service layer พร้อม logging

### Razor Pages

- **One PageModel per feature** — ไม่ควรมี PageModel ใหญ่เกินไป แยก Service ออกมา
- **ViewModel pattern:** สร้าง ViewModel สำหรับ binding form แทนการใช้ Model ตรง ๆ
- **CSRF:** ทุก Form ต้องมี `@Html.AntiForgeryToken()`

### Frontend

- **Tailwind CSS:** ใช้ utility classes ไม่เขียน custom CSS เว้นแต่จำเป็น
- **HTMX:** สำหรับ partial update, form submission แบบ AJAX
- **JavaScript:** เขียนน้อยที่สุด ใช้ HTMX attributes เป็นหลัก

## Architecture Rules

- **Service Layer Pattern:** Business logic อยู่ใน `Services/` ไม่ใช่ใน PageModel
- **Repository Pattern สำหรับ Dapper queries:** Query ที่ซับซ้อนแยกไปที่ Repository
- **Separation of Concerns:** เรื่องทั่วไปกับเรื่องทุจริตไม่ share Service — แยกสมบูรณ์

## Testing Strategy

### Unit Tests (ใช้ xUnit)

- ทดสอบ Services logic ทั้งหมด
- Mock DbContext ด้วย InMemory Database หรือ Moq

### Integration Tests

- ทดสอบ API endpoints ด้วย WebApplicationFactory
- ทดสอบ Razor Pages ที่มี database interaction

### Manual Testing Checklist

- [ ] ยื่นเรื่องทั่วไป → รับ SMS/Email
- [ ] ยื่นเรื่องทุจริต → ข้อมูลถูก Mask
- [ ] เจ้าหน้าที่รับเรื่อง → Auto-assign ถ้าไม่มีคนรับ
- [ ] Super Admin สร้าง API Key → Test ด้วย Postman

## Build & Run

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (Development)
dotnet run --project SRT.Complaint

# Run migrations
dotnet ef database update --project SRT.Complaint

# Create migration
dotnet ef migrations add MigrationName --project SRT.Complaint
```

## Common Tasks

### สร้าง Razor Page ใหม่

```bash
# สร้าง Page พร้อม PageModel
dotnet new page -n PageName -o Pages/FolderName
```

### เพิ่ม NuGet Package

```bash
dotnet add package PackageName
```

### สร้าง Service ใหม่

1. สร้าง Interface ใน `Services/IServiceName.cs`
2. สร้าง Implementation ใน `Services/ServiceName.cs`
3. Register ใน `Program.cs`: `builder.Services.AddScoped<IServiceName, ServiceName>()`

## Security Checklist

- [ ] Password hashing: BCrypt cost factor 12
- [ ] ข้อมูลทุจริต: AES-256-CBC encryption
- [ ] Session: HttpOnly + Secure + SameSite=Strict
- [ ] CSRF: Anti-forgery token ทุก Form
- [ ] File upload: ตรวจ MIME type + Extension whitelist
- [ ] SQL Injection: Parameterized query เสมอ
- [ ] Rate Limiting: จำกัด Submit 5 ครั้ง/IP/ชั่วโมง

## API Development

### Endpoint Structure

```
POST   /api/complaints                    (Scope: complaints:write)
GET    /api/complaints/{ref}              (Scope: complaints:read)
GET    /api/complaints/{ref}/edoc-payload (Scope: complaints:edoc)
```

### API Key Validation Flow

1. อ่าน Key จาก Header `X-API-Key`
2. Hash Key → เทียบกับ DB
3. ตรวจ IsActive + ExpiresAt
4. ตรวจ IP Whitelist
5. ตรวจ Rate Limit (Sliding Window 1 นาที)
6. ตรวจ Scope
7. บันทึก ApiRequestLog

## Deployment (IIS on GDCC)

### Pre-deployment Checklist

- [ ] ได้ Connection String จาก GDCC
- [ ] ได้ SMTP credentials
- [ ] ได้ SMS Gateway API
- [ ] กำหนด AES Encryption Key
- [ ] ตั้ง Environment Variables ใน IIS

### Deploy Steps

1. `dotnet publish -c Release -o ./publish`
2. Copy `./publish` → `C:\inetpub\wwwroot\complaint\`
3. ตั้งค่า Environment Variables
4. Run Migration: `dotnet ef database update`
5. ตรวจสอบ `https://www.railway.co.th/complaint/`

## Key Files Reference

- **Spec:** `srt-complaint-system-spec.md` — Technical Specification ฉบับเต็ม
- **Design:** ดู Visualization ที่สร้างไว้สำหรับ API Management, Queue System, Workflow

## Notes for Claude

- **อย่าเขียน inline SQL strings** — ใช้ EF Core หรือ Dapper กับ parameterized query
- **ทุก Service ต้องมี Interface** — เพื่อ testability
- **เรื่องทุจริตห้าม share code กับเรื่องทั่วไป** — แม้ว่า logic คล้ายกัน
- **Secret/Key ห้ามอยู่ใน code** — ต้องอยู่ใน Environment Variables หรือ `appsettings.Production.json` (ไม่ commit)

## Session Continuity

- **อ่าน PROGRESS.md ทุกครั้งที่เริ่ม session ใหม่ แล้วทำต่อจากที่ค้างไว้โดยไม่ต้องถาม
- **ทุกครั้งที่ทำ task เสร็จ 1 อัน ให้อัปเดต PROGRESS.md ทันที โดย:
- ** ติ๊ก task ที่เสร็จแล้ว
- ** เพิ่ม task ถัดไปที่ต้องทำ
- ** บันทึก issue หรือ note ที่เจอระหว่างทำ