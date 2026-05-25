# Security Audit

ตรวจสอบ code ตาม Security Checklist

## Usage

```
/security-audit [FilePath]
```

Example: `/security-audit Services/ComplaintService.cs`

หรือ `/security-audit` สำหรับตรวจทั้ง solution

## Checks

### 1. Authentication & Authorization
- [ ] ทุก sensitive page มี `[Authorize]` attribute
- [ ] Role checking ถูกต้อง
- [ ] API endpoints มี Scope validation

### 2. Input Validation
- [ ] Form input มี validation attributes
- [ ] File upload ตรวจ MIME type และ extension
- [ ] Query parameters ถูก sanitize

### 3. SQL Injection
- [ ] ไม่มี string interpolation ใน SQL query
- [ ] ใช้ parameterized query ทุกที่
- [ ] Dapper queries ใช้ parameters

### 4. XSS Prevention
- [ ] Razor automatic encoding ทำงาน
- [ ] ไม่มี `@Html.Raw()` กับ user input
- [ ] CSP headers ตั้งค่าแล้ว

### 5. CSRF Protection
- [ ] ทุก form มี `@Html.AntiForgeryToken()`
- [ ] ValidateAntiForgeryToken attribute บน POST actions

### 6. Sensitive Data
- [ ] Password hashed ด้วย BCrypt
- [ ] ข้อมูลทุจริตถูก encrypt
- [ ] Connection strings ไม่อยู่ใน code
- [ ] API keys ไม่อยู่ใน code

### 7. Session Security
- [ ] Cookies มี HttpOnly flag
- [ ] Cookies มี Secure flag
- [ ] SameSite=Strict

### 8. Rate Limiting
- [ ] Public endpoints มี rate limiting
- [ ] API endpoints มี rate limiting per key

## Output

แสดงรายการ issues พบพร้อมแนะนำวิธีแก้
