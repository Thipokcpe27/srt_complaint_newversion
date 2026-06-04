# คู่มือ Deploy ระบบรับเรื่องร้องเรียน รฟท. บน IIS (Windows Server)

> เอกสารนี้เขียนสำหรับผู้ที่ไม่เคย Deploy .NET บน IIS มาก่อน อ่านทีละขั้นตอน ห้ามข้าม

---

## สารบัญ

1. [สิ่งที่ต้องเตรียมบนเครื่อง Server](#1-สิ่งที่ต้องเตรียมบนเครื่อง-server)
2. [ติดตั้ง SQL Server](#2-ติดตั้ง-sql-server)
3. [ตั้งค่า SQL Server สำหรับระบบนี้](#3-ตั้งค่า-sql-server-สำหรับระบบนี้)
4. [เตรียม Source Code และ Build](#4-เตรียม-source-code-และ-build)
5. [สร้าง Database และ Run Migration](#5-สร้าง-database-และ-run-migration)
6. [วาง Files บน Server](#6-วาง-files-บน-server)
7. [ตั้งค่า IIS](#7-ตั้งค่า-iis)
8. [ตั้งค่า Environment Variables (Secrets)](#8-ตั้งค่า-environment-variables-secrets)
9. [ตั้งค่า appsettings.Production.json](#9-ตั้งค่า-appsettingsproductionjson)
10. [ตั้งค่า Folder สำหรับ File Upload และ Logs](#10-ตั้งค่า-folder-สำหรับ-file-upload-และ-logs)
11. [เปิดใช้งานและทดสอบ](#11-เปิดใช้งานและทดสอบ)
12. [Login ครั้งแรก](#12-login-ครั้งแรก)
13. [Checklist ก่อน Go-Live](#13-checklist-ก่อน-go-live)
14. [แก้ปัญหาที่พบบ่อย](#14-แก้ปัญหาที่พบบ่อย)

---

## 1. สิ่งที่ต้องเตรียมบนเครื่อง Server

### 1.1 ข้อมูลที่ต้องได้จาก GDCC/ทีม IT ก่อน

| รายการ | ตัวอย่าง | หมายเหตุ |
|--------|----------|----------|
| Connection String ของ SQL Server | `Server=db01;Database=SrtComplaint;...` | ขอจาก GDCC |
| SMTP Host/User/Password | smtp.railway.co.th | สำหรับส่ง Email |
| SMS Gateway URL + API Key | https://sms.example.com/send | สำหรับส่ง SMS |
| Cloudflare Turnstile Site Key + Secret Key | - | ป้องกัน Bot บน Form |
| URL ที่จะ Deploy | https://www.railway.co.th/complaint/ | กำหนดก่อน config IIS |

### 1.2 Software ที่ต้องติดตั้งบน Server

ติดตั้งตามลำดับนี้:

**ก. .NET 9 Hosting Bundle**
- ดาวน์โหลด: https://dotnet.microsoft.com/download/dotnet/9.0
- เลือก **"ASP.NET Core Runtime 9.x.x — Hosting Bundle"** (ไม่ใช่ SDK)
- ติดตั้งแล้ว Restart เครื่อง

**ข. SQL Server** (ถ้ายังไม่มี — ดูหัวข้อ 2)

**ค. Node.js (ใช้ตอน Build เท่านั้น — ไม่ต้องติดตั้งบน Server จริง)**
- ติดตั้งบนเครื่อง Developer ที่จะ Build
- ดาวน์โหลด: https://nodejs.org (LTS version)

**ง. .NET SDK 9** (ติดตั้งบนเครื่อง Developer)
- ดาวน์โหลด: https://dotnet.microsoft.com/download/dotnet/9.0
- เลือก **"SDK"**

**ตรวจสอบว่าติดตั้งสำเร็จ** (รันใน PowerShell บนเครื่อง Developer):
```powershell
dotnet --version
# ต้องขึ้น 9.x.x

node --version
# ต้องขึ้น v20.x หรือสูงกว่า

npm --version
# ต้องขึ้น 10.x หรือสูงกว่า
```

---

## 2. ติดตั้ง SQL Server

> ถ้า GDCC จัดให้ SQL Server มาให้แล้ว ข้ามไปหัวข้อ 3 ได้เลย

### 2.1 ดาวน์โหลด SQL Server

- ใช้ **SQL Server 2022 Express** (ฟรี) หรือ **Standard/Enterprise** ที่ GDCC มีลิขสิทธิ์
- Express: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

### 2.2 ติดตั้ง SQL Server

1. รัน Installer เลือก **"Basic"** installation (ง่ายที่สุด)
2. ระหว่างติดตั้งจะถามหลายอย่าง — ใช้ค่า Default ทั้งหมดยกเว้น:
   - **Authentication Mode:** เลือก **"Mixed Mode"** (SQL Server and Windows Authentication)
   - **sa password:** ตั้งรหัสผ่านที่แข็งแรง เช่น `SRT@Comp2024!` — **จดเก็บไว้**
3. ติดตั้งเสร็จกด Finish

### 2.3 ติดตั้ง SQL Server Management Studio (SSMS)

- ดาวน์โหลด: https://aka.ms/ssmsfullsetup
- ใช้สำหรับจัดการ Database ด้วย GUI
- ติดตั้งตามปกติ Next → Next → Install

### 2.4 เปิด TCP/IP ให้ SQL Server

1. เปิด **SQL Server Configuration Manager** (ค้นหาใน Start Menu)
2. ไปที่ **SQL Server Network Configuration → Protocols for MSSQLSERVER**
3. คลิกขวา **TCP/IP** → **Enable**
4. คลิกขวา **TCP/IP** → **Properties** → แท็บ **IP Addresses**
5. เลื่อนลงไปที่ **IPAll** → ตั้ง **TCP Port = 1433**
6. กด OK → Restart SQL Server Service

```powershell
# Restart SQL Server Service ใน PowerShell (รันในฐานะ Administrator)
Restart-Service -Name MSSQLSERVER
```

---

## 3. ตั้งค่า SQL Server สำหรับระบบนี้

เปิด **SSMS** แล้วต่อเข้า SQL Server ด้วย Windows Authentication หรือ sa

### 3.1 สร้าง Database

รัน SQL ต่อไปนี้ใน SSMS (New Query):

```sql
-- สร้าง Database หลัก
CREATE DATABASE SrtComplaint
    COLLATE Thai_CI_AS;  -- รองรับภาษาไทย
GO

USE SrtComplaint;
GO

-- สร้าง Schema สำหรับเรื่องทุจริต (แยกจาก dbo)
CREATE SCHEMA corruption;
GO
```

### 3.2 สร้าง SQL Login สำหรับ Application

อย่าใช้ `sa` โดยตรง — สร้าง user แยกสำหรับ app:

```sql
-- สร้าง Login ระดับ Server
CREATE LOGIN srt_app_user
    WITH PASSWORD = 'SrtApp@2024SecurePass!',
         CHECK_EXPIRATION = OFF,
         CHECK_POLICY = ON;
GO

-- สร้าง User ใน Database
USE SrtComplaint;
GO

CREATE USER srt_app_user FOR LOGIN srt_app_user;
GO

-- ให้สิทธิ์ที่จำเป็น (db_owner ระหว่าง setup, จะลดสิทธิ์ได้หลัง Migration เสร็จ)
ALTER ROLE db_owner ADD MEMBER srt_app_user;
GO
```

> **หมายเหตุ:** หลังจาก Run Migration เสร็จแล้ว แนะนำลดสิทธิ์เหลือเฉพาะที่จำเป็น (ดูหัวข้อ 13)

### 3.3 Connection String ที่ได้

จด Connection String นี้ไว้ใช้ต่อ:

```
Server=localhost,1433;Database=SrtComplaint;User Id=srt_app_user;Password=SrtApp@2024SecurePass!;TrustServerCertificate=True;
```

> ถ้า SQL Server อยู่คนละเครื่อง เปลี่ยน `localhost` เป็น IP หรือ hostname ของ SQL Server

---

## 4. เตรียม Source Code และ Build

ทำบนเครื่อง **Developer** (ไม่ใช่ Server)

### 4.1 เตรียม Source Code

```powershell
# ไปที่ folder โปรเจกต์
cd C:\Users\thipo\srt_complaint_newversion

# ตรวจสอบว่า Node dependencies มีครบ
cd SRT.Complaint
npm install
cd ..
```

### 4.2 Publish (Build สำหรับ Production)

```powershell
# รันจาก root folder ของโปรเจกต์
dotnet publish SRT.Complaint/SRT.Complaint.csproj `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o ./publish

# ถ้าสำเร็จจะมี folder ./publish ที่มีไฟล์ทั้งหมด
```

> **ถ้า Build ไม่ผ่าน** เพราะ Tailwind CSS: ตรวจสอบว่า `npm install` ทำในโฟลเดอร์ `SRT.Complaint/` แล้ว

### 4.3 ตรวจสอบผลการ Build

```powershell
# ดูว่า publish folder มีไฟล์หลักครบ
ls ./publish | Select-Object Name
```

ต้องมีไฟล์เหล่านี้:
- `SRT.Complaint.exe`
- `SRT.Complaint.dll`
- `web.config`
- `wwwroot/` (มี css/, js/, lib/)
- `appsettings.json`

---

## 5. สร้าง Database และ Run Migration

ยังทำบนเครื่อง **Developer** แต่เชื่อมไปยัง SQL Server จริง (Production DB)

### 5.1 ติดตั้ง EF Core Tools (ถ้ายังไม่มี)

```powershell
dotnet tool install --global dotnet-ef
# หรืออัปเดต
dotnet tool update --global dotnet-ef
```

### 5.2 ตั้ง Connection String ชั่วคราวสำหรับ Migration

สร้างไฟล์ `SRT.Complaint/appsettings.Migration.json` (ไม่ต้อง commit):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<IP_SERVER>,1433;Database=SrtComplaint;User Id=srt_app_user;Password=SrtApp@2024SecurePass!;TrustServerCertificate=True;"
  }
}
```

### 5.3 สร้าง Migration (ถ้ายังไม่มี)

```powershell
cd C:\Users\thipo\srt_complaint_newversion

# Migration สำหรับ AppDbContext (ตาราง dbo.*)
dotnet ef migrations add InitialCreate `
    --project SRT.Complaint `
    --context AppDbContext `
    --output-dir Migrations/App

# Migration สำหรับ CorruptionDbContext (ตาราง corruption.*)
dotnet ef migrations add InitialCreate `
    --project SRT.Complaint `
    --context CorruptionDbContext `
    --output-dir Migrations/Corruption
```

### 5.4 วิเคราะห์ Migration ก่อน Apply

**ต้องอ่านไฟล์ Migration ก่อนเสมอ** — เปิดดูใน `SRT.Complaint/Migrations/App/` และ `SRT.Complaint/Migrations/Corruption/`

ตรวจสอบ:
- ไม่มี Column type เปลี่ยนแบบทำลายข้อมูล
- FK ที่มี Cascade ไม่วนซ้ำ (multiple cascade paths)
- ไม่มี Index ซ้ำ

### 5.5 Apply Migration ไปยัง Production Database

```powershell
# ตั้ง Environment Variable ชี้ไป Production Connection String
$env:ConnectionStrings__DefaultConnection = "Server=<IP_SERVER>,1433;Database=SrtComplaint;User Id=srt_app_user;Password=SrtApp@2024SecurePass!;TrustServerCertificate=True;"

# Apply AppDbContext
dotnet ef database update `
    --project SRT.Complaint `
    --context AppDbContext

# Apply CorruptionDbContext
dotnet ef database update `
    --project SRT.Complaint `
    --context CorruptionDbContext
```

### 5.6 ตรวจสอบใน SSMS

เปิด SSMS → ต่อ Database → ดู Tables ต้องมี:
- `dbo.StaffUsers`
- `dbo.Complaints`
- `dbo.ComplaintCategories`
- `dbo.ApiKeys`
- `corruption.Reports`
- `corruption.InvestigationLogs`
- และตารางอื่น ๆ อีกหลายตาราง

---

## 6. วาง Files บน Server

### 6.1 Copy Publish Folder ไปยัง Server

```powershell
# Copy จาก Developer ไป Server (ปรับ path ตามจริง)
# วิธี 1: ใช้ robocopy (ถ้าอยู่ใน Network เดียวกัน)
robocopy ".\publish" "\\SERVER_NAME\inetpub\wwwroot\complaint" /MIR /NFL /NDL

# วิธี 2: Zip แล้ว Copy ด้วยมือ
Compress-Archive -Path ".\publish\*" -DestinationPath ".\publish.zip"
# แล้ว Copy publish.zip ไปที่ Server แล้ว Extract
```

### 6.2 โครงสร้าง Folder บน Server

```
C:\inetpub\wwwroot\complaint\        ← Application root
├── SRT.Complaint.exe
├── SRT.Complaint.dll
├── web.config
├── appsettings.json
├── appsettings.Production.json      ← สร้างใหม่บน Server (ดูหัวข้อ 9)
├── wwwroot\
│   ├── css\
│   ├── js\
│   └── lib\
├── Fonts\
│   ├── Sarabun-Regular.ttf
│   └── Sarabun-Bold.ttf
├── logs\                            ← สร้าง folder นี้ด้วยมือ
└── uploads\                         ← สร้าง folder นี้ด้วยมือ
```

สร้าง folder ที่จำเป็น:
```powershell
# รันบน Server ในฐานะ Administrator
New-Item -ItemType Directory -Force -Path "C:\inetpub\wwwroot\complaint\logs"
New-Item -ItemType Directory -Force -Path "C:\inetpub\wwwroot\complaint\uploads"
```

---

## 7. ตั้งค่า IIS

### 7.1 เปิดใช้งาน IIS (ถ้ายังไม่ได้เปิด)

```powershell
# รันใน PowerShell (Administrator) บน Server
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIExtensions
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIFilter
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45
```

หรือผ่าน GUI: **Control Panel → Programs → Turn Windows features on or off → Internet Information Services**

### 7.2 ติดตั้ง ASP.NET Core Module

ต้องติดตั้ง **.NET 9 Hosting Bundle** ก่อน (หัวข้อ 1.2) เพื่อให้ได้ ANCM (ASP.NET Core Module)

ตรวจสอบว่าติดตั้งแล้ว:
```powershell
Get-WebConfiguration "system.webServer/globalModules/*" | Where-Object { $_.name -like "*AspNet*" }
# ต้องขึ้น AspNetCoreModuleV2
```

### 7.3 สร้าง Application Pool

1. เปิด **IIS Manager** (inetmgr)
2. ซ้าย: คลิก **Application Pools**
3. ขวา: **Add Application Pool...**
4. ตั้งค่า:
   - **Name:** `SrtComplaintPool`
   - **.NET CLR version:** `No Managed Code` ← สำคัญมาก
   - **Managed pipeline mode:** `Integrated`
5. คลิก OK

จากนั้นตั้งค่า Pool เพิ่มเติม:
1. คลิกขวา `SrtComplaintPool` → **Advanced Settings**
2. ตั้งค่า:
   - **Identity:** เปลี่ยนจาก `ApplicationPoolIdentity` เป็น `LocalSystem` (ง่ายที่สุดตอน Setup) หรือสร้าง Service Account แยก (ปลอดภัยกว่า)
   - **Start Mode:** `AlwaysRunning`
   - **Idle Time-out (minutes):** `0` (ไม่ให้หยุด)
   - **Regular Time Interval (minutes):** `1440` (Recycle วันละครั้ง)

### 7.4 สร้าง Application ใน IIS

**กรณี A: Deploy เป็น Sub-application ใต้ Default Web Site**

1. IIS Manager → ซ้าย: **Default Web Site**
2. ขวา: **Add Application...**
3. ตั้งค่า:
   - **Alias:** `complaint`
   - **Application pool:** `SrtComplaintPool`
   - **Physical path:** `C:\inetpub\wwwroot\complaint`
4. คลิก OK

URL จะเป็น: `https://www.railway.co.th/complaint/`

**กรณี B: Deploy เป็น Website แยก (Root)**

1. IIS Manager → ซ้าย: คลิกขวา **Sites** → **Add Website...**
2. ตั้งค่า:
   - **Site name:** `SrtComplaint`
   - **Application pool:** `SrtComplaintPool`
   - **Physical path:** `C:\inetpub\wwwroot\complaint`
   - **Binding:** `https`, Port `443`, Hostname `www.railway.co.th`
3. คลิก OK

### 7.5 ตั้งค่า HTTPS Certificate

1. IIS Manager → เลือก Site/Server → **Server Certificates**
2. Import Certificate จาก GDCC หรือใช้ Let's Encrypt
3. ใน Binding ของ Site → เพิ่ม HTTPS Binding → เลือก Certificate

### 7.6 ตรวจสอบ web.config

ไฟล์ `web.config` ใน publish folder ควรมีเนื้อหาแบบนี้ (จะถูกสร้างอัตโนมัติตอน publish):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*"
             modules="AspNetCoreModuleV2"
             resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\SRT.Complaint.dll"
                  stdoutLogEnabled="false"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

---

## 8. ตั้งค่า Environment Variables (Secrets)

Environment Variables ที่เก็บ **Secrets** ไม่ควร commit ลง Git ตั้งค่าใน IIS แทน

### 8.1 วิธีตั้ง Environment Variables ใน IIS

**วิธีที่ 1: ผ่าน IIS Manager (แนะนำ)**

1. IIS Manager → เลือก Application `complaint`
2. ตรงกลาง: ดับเบิ้ลคลิก **Configuration Editor**
3. Section: `system.webServer/aspNetCore`
4. คลิก `environmentVariables` → คลิก `...` ด้านขวา
5. เพิ่ม Variables ต่อไปนี้ทีละตัว

**วิธีที่ 2: แก้ไข web.config โดยตรง**

เพิ่ม `<environmentVariable>` ใน `web.config`:

```xml
<aspNetCore processPath="dotnet" arguments=".\SRT.Complaint.dll"
            stdoutLogEnabled="false"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />

    <!-- Database -->
    <environmentVariable name="ConnectionStrings__DefaultConnection"
      value="Server=localhost,1433;Database=SrtComplaint;User Id=srt_app_user;Password=SrtApp@2024SecurePass!;TrustServerCertificate=True;" />

    <!-- Encryption Key (AES-256 — ต้องเป็น Base64 ของ 32 bytes) -->
    <environmentVariable name="Encryption__Key"
      value="YOUR_BASE64_ENCRYPTION_KEY_HERE" />

    <!-- SMTP -->
    <environmentVariable name="Notifications__SmtpHost" value="smtp.example.com" />
    <environmentVariable name="Notifications__SmtpPort" value="587" />
    <environmentVariable name="Notifications__SmtpUser" value="noreply@railway.co.th" />
    <environmentVariable name="Notifications__SmtpPassword" value="YOUR_SMTP_PASSWORD" />

    <!-- SMS Gateway -->
    <environmentVariable name="Notifications__SmsGatewayUrl" value="https://sms.example.com/send" />
    <environmentVariable name="Notifications__SmsApiKey" value="YOUR_SMS_API_KEY" />

    <!-- Cloudflare Turnstile (CAPTCHA) -->
    <environmentVariable name="Turnstile__SiteKey" value="YOUR_TURNSTILE_SITE_KEY" />
    <environmentVariable name="Turnstile__SecretKey" value="YOUR_TURNSTILE_SECRET_KEY" />

    <!-- File Upload Path -->
    <environmentVariable name="FileUpload__StoragePath" value="C:\inetpub\wwwroot\complaint\uploads" />

    <!-- Traffy Fondue (ถ้าใช้) -->
    <environmentVariable name="TraffyFondue__Username" value="" />
    <environmentVariable name="TraffyFondue__Password" value="" />
    <environmentVariable name="TraffyFondue__OrgId" value="" />
    <environmentVariable name="TraffyFondue__WebhookSecret" value="" />
  </environmentVariables>
</aspNetCore>
```

### 8.2 สร้าง Encryption Key

รันบนเครื่องใดก็ได้ที่มี PowerShell:

```powershell
# สร้าง Random Key 32 bytes → Base64
$key = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
Write-Output "Encryption Key: $key"
```

ผลลัพธ์ประมาณ: `xK3mP9qRv2Yw8Tz6Nj4Ld7Uc1Fs5Bo0Xi+Ae=`

> **สำคัญมาก:** Key นี้ใช้ encrypt ข้อมูลทุจริตและรหัสผ่านชั่วคราว ถ้าเปลี่ยน Key ทีหลังจะ decrypt ข้อมูลเก่าไม่ได้ ต้องเก็บ Key นี้ไว้ในที่ปลอดภัย

---

## 9. ตั้งค่า appsettings.Production.json

สร้างไฟล์นี้บน Server ที่ `C:\inetpub\wwwroot\complaint\appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Security": {
    "SubmitLimitPerHour": 5,
    "LoginLimitPerWindow": 10,
    "LoginWindowMinutes": 15,
    "TrackVerifyLimitPerWindow": 10,
    "TrackVerifyWindowMinutes": 15,
    "SessionTimeoutMinutes": 30
  },
  "AuditLog": {
    "RetentionDays": 90
  },
  "ApiManagement": {
    "WebhookSigningKeyPrefix": "SRT_WHK_",
    "RequestLogRetentionDays": 90,
    "RateLimitWindowSeconds": 60
  }
}
```

> Secrets (Connection String, Encryption Key, Passwords) อยู่ใน Environment Variables แล้ว (หัวข้อ 8) ไม่ต้องใส่ในไฟล์นี้

---

## 10. ตั้งค่า Folder สำหรับ File Upload และ Logs

Application Pool Identity ต้องมีสิทธิ์เขียน Folder เหล่านี้:

```powershell
# รันบน Server ในฐานะ Administrator

# สิทธิ์ Logs folder
icacls "C:\inetpub\wwwroot\complaint\logs" /grant "IIS AppPool\SrtComplaintPool:(OI)(CI)F"

# สิทธิ์ Uploads folder
icacls "C:\inetpub\wwwroot\complaint\uploads" /grant "IIS AppPool\SrtComplaintPool:(OI)(CI)F"

# ถ้าใช้ LocalSystem Identity ข้างต้นอาจไม่จำเป็น แต่ทำไว้ก็ดี
```

**ถ้าเลือก Identity เป็น LocalSystem** ไม่ต้องตั้ง icacls เพราะ LocalSystem มีสิทธิ์ทุกอย่างบนเครื่องอยู่แล้ว

---

## 11. เปิดใช้งานและทดสอบ

### 11.1 เริ่มต้น Application Pool และ Site

```powershell
# รันใน PowerShell (Administrator) บน Server
Import-Module WebAdministration

# Start Application Pool
Start-WebAppPool -Name "SrtComplaintPool"

# Start Website
Start-Website -Name "Default Web Site"  # หรือชื่อ Site ที่สร้าง
```

หรือใน IIS Manager → คลิกขวา Pool/Site → Start

### 11.2 ดู Startup Log

```powershell
# ดู Log ล่าสุด (Application เริ่มต้นจะ Log ออกมา)
Get-Content "C:\inetpub\wwwroot\complaint\logs\srt-complaint-*.log" -Tail 50
```

ต้องไม่มี Error สีแดง โดยเฉพาะ:
- `ConnectionStrings:DefaultConnection ยังไม่ได้ตั้งค่า` → ตั้ง Environment Variable ไม่สำเร็จ
- `Encryption:Key ยังไม่ได้ตั้งค่า` → ตั้ง Encryption Key ไม่สำเร็จ

### 11.3 ทดสอบ HTTP

```powershell
# ทดสอบ Local บน Server
Invoke-WebRequest -Uri "http://localhost/complaint/" -UseBasicParsing
# ต้องได้ StatusCode 200
```

### 11.4 เปิด Browser ทดสอบ

เปิด Browser ไปที่ `https://www.railway.co.th/complaint/` (หรือ URL ที่กำหนด)

ต้องเห็น:
- หน้า Landing Page ของระบบ
- ไม่มี Error 500
- ไม่มี "502 Bad Gateway" หรือ "503 Service Unavailable"

---

## 12. Login ครั้งแรก

### 12.1 รหัสผ่าน SuperAdmin

ตอน App เริ่มครั้งแรก ระบบจะ **สร้าง SuperAdmin อัตโนมัติ** และ Log รหัสผ่านออกมา

```powershell
# ดู Log เพื่อหารหัสผ่าน (มีแค่ครั้งเดียว)
Select-String -Path "C:\inetpub\wwwroot\complaint\logs\*.log" -Pattern "SuperAdmin สร้างครั้งแรก"
```

จะเห็นบรรทัดแบบนี้:
```
[WRN] ⚠️  SuperAdmin สร้างครั้งแรก — รหัสผ่านชั่วคราว (ใช้ได้ครั้งเดียว): AbCd1234XyZw== — เปลี่ยนทันทีหลัง login
```

### 12.2 Login และเปลี่ยนรหัสผ่าน

1. เปิด Browser ไปที่ `/Staff/Login`
2. **รหัสพนักงาน:** `0000001`
3. **รหัสผ่าน:** รหัสที่เห็นใน Log
4. ระบบจะบังคับเปลี่ยนรหัสผ่านทันที → ตั้งรหัสผ่านใหม่ที่แข็งแรง

### 12.3 ตั้งค่าหลังจาก Login

ไปที่ Admin Panel เพื่อ:
1. **ตั้งค่าหมวดหมู่** (Categories/SubCategories)
2. **ตั้งค่า SLA** (เวลาดำเนินการ)
3. **เพิ่มเจ้าหน้าที่** (Staff Users)
4. **ตั้งค่า Notification Templates** (SMS/Email)

---

## 13. Checklist ก่อน Go-Live

### ด้านความปลอดภัย

- [ ] เปลี่ยนรหัสผ่าน SuperAdmin แล้ว
- [ ] ลด SQL Permission จาก `db_owner` เหลือเฉพาะที่จำเป็น:
  ```sql
  USE SrtComplaint;
  ALTER ROLE db_owner DROP MEMBER srt_app_user;
  ALTER ROLE db_datareader ADD MEMBER srt_app_user;
  ALTER ROLE db_datawriter ADD MEMBER srt_app_user;
  -- Grant Execute สำหรับ Stored Procedures (ถ้ามี)
  GRANT EXECUTE ON SCHEMA::dbo TO srt_app_user;
  GRANT EXECUTE ON SCHEMA::corruption TO srt_app_user;
  ```
- [ ] HTTPS ใช้งานได้ ไม่มี Certificate Warning
- [ ] HTTP Redirect ไป HTTPS อัตโนมัติ
- [ ] `ASPNETCORE_ENVIRONMENT` ตั้งเป็น `Production`
- [ ] ไม่มีไฟล์ `appsettings.Development.json` บน Server
- [ ] Firewall เปิดเฉพาะ Port 80, 443 เท่านั้น (ปิด 1433 จากภายนอก)
- [ ] Turnstile (CAPTCHA) ทดสอบแล้วใช้งานได้

### ด้านการทำงาน

- [ ] ยื่นเรื่องทั่วไปได้ → รับ SMS และ Email
- [ ] ยื่นเรื่องทุจริตได้ → ข้อมูลถูก Mask
- [ ] Login เจ้าหน้าที่ได้
- [ ] ดู PDF เอกสารได้
- [ ] Upload ไฟล์แนบได้ (สูงสุด 10 MB/ไฟล์, 5 ไฟล์)
- [ ] Log ถูกเขียนที่ `logs/` folder

### ด้าน Performance

- [ ] Application Pool ตั้ง `Start Mode: AlwaysRunning`
- [ ] Idle Time-out ตั้งเป็น 0
- [ ] ทดสอบ Response Time < 3 วินาที

---

## 14. แก้ปัญหาที่พบบ่อย

### Error 500.30 — ASP.NET Core app failed to start

**สาเหตุ:** App ไม่สามารถ Start ได้ มักเกิดจาก Config ผิด

```powershell
# เปิด stdout log ชั่วคราวเพื่อดู Error
# แก้ใน web.config: stdoutLogEnabled="true"
# หลังดู Error แล้วให้ปิดกลับ stdoutLogEnabled="false"
```

**สาเหตุที่พบบ่อย:**
- Connection String ผิด → ตรวจ Environment Variable
- Encryption Key ไม่ได้ตั้ง → ตรวจ `Encryption__Key`
- .NET 9 Hosting Bundle ไม่ได้ติดตั้ง → ติดตั้งใหม่แล้ว Restart IIS

### Error 502 Bad Gateway

**สาเหตุ:** Application Pool ไม่ทำงาน

```powershell
# ตรวจสอบสถานะ Pool
Get-WebConfiguration "system.applicationHost/applicationPools/add[@name='SrtComplaintPool']" | Select-Object state
# Restart Pool
Restart-WebAppPool -Name "SrtComplaintPool"
```

### Cannot connect to SQL Server

```powershell
# ทดสอบ Connection จาก Server
sqlcmd -S localhost,1433 -U srt_app_user -P "SrtApp@2024SecurePass!" -Q "SELECT 1"
# ถ้าไม่ได้ → ตรวจ Firewall, SQL Server Service, TCP/IP
```

### ไม่รับ Environment Variables

ตรวจว่า Syntax ถูก — ASP.NET Core ใช้ `__` (Double Underscore) แทน `:` ใน nested config:
- `ConnectionStrings:DefaultConnection` → `ConnectionStrings__DefaultConnection`
- `Encryption:Key` → `Encryption__Key`

### ไฟล์ Upload ไม่ได้

```powershell
# ตรวจสิทธิ์ folder
icacls "C:\inetpub\wwwroot\complaint\uploads"
# ต้องมี IIS AppPool\SrtComplaintPool หรือ SYSTEM มีสิทธิ์ Write
```

### Log ไม่ถูกเขียน

```powershell
# ตรวจสิทธิ์ logs folder
icacls "C:\inetpub\wwwroot\complaint\logs"
# ให้สิทธิ์ถ้าไม่มี
icacls "C:\inetpub\wwwroot\complaint\logs" /grant "IIS AppPool\SrtComplaintPool:(OI)(CI)F"
```

---

## Quick Reference — Commands สำคัญ

```powershell
# === Build & Deploy ===
# Build
dotnet publish SRT.Complaint/SRT.Complaint.csproj -c Release -r win-x64 --self-contained false -o ./publish

# Run Migration
dotnet ef database update --project SRT.Complaint --context AppDbContext
dotnet ef database update --project SRT.Complaint --context CorruptionDbContext

# === IIS Management (บน Server) ===
# Restart Application Pool
Restart-WebAppPool -Name "SrtComplaintPool"

# ดู App Pool Status
Get-WebConfiguration "system.applicationHost/applicationPools/add[@name='SrtComplaintPool']"

# === Logs ===
# ดู Log ล่าสุด
Get-Content "C:\inetpub\wwwroot\complaint\logs\srt-complaint-*.log" -Tail 100

# หา Error ใน Log
Select-String -Path "C:\inetpub\wwwroot\complaint\logs\*.log" -Pattern "ERR|FATAL|Exception"

# === SQL Server ===
# Test Connection
sqlcmd -S localhost,1433 -U srt_app_user -P "YOUR_PASSWORD" -Q "SELECT DB_NAME()"

# สร้าง Encryption Key ใหม่
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

---

*เอกสารนี้ตรงกับ Source Code ณ วันที่ 4 มิถุนายน 2566 — ถ้ามี Migration หรือ Config ใหม่เพิ่มทีหลัง ให้อัปเดตเอกสารด้วย*
