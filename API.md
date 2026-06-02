# REST API — ระบบรับเรื่องร้องเรียน รฟท.

เอกสารนี้อธิบาย REST API ของระบบรับเรื่องร้องเรียนออนไลน์ การรถไฟแห่งประเทศไทย  
สำหรับระบบภายนอกที่ต้องการเชื่อมต่อ เช่น e-Document, ระบบ Dashboard, หรือ Third-party Integrations

---

## สารบัญ

- [Base URL](#base-url)
- [Authentication](#authentication)
- [Scopes (สิทธิ์)](#scopes-สิทธิ์)
- [Rate Limiting](#rate-limiting)
- [Error Responses](#error-responses)
- [Complaints API](#complaints-api)
- [Statistics API](#statistics-api)
- [Webhooks API](#webhooks-api)
- [Webhook Events (Outbound)](#webhook-events-outbound)
- [Traffy Fondue Webhook Receiver](#traffy-fondue-webhook-receiver)

---

## Base URL

```
https://www.railway.co.th/complaint
```

ตัวอย่าง: `https://www.railway.co.th/complaint/api/complaints/SRT-COMPL-2568-0001`

---

## Authentication

ทุก request ต้องส่ง API Key ใน HTTP Header:

```
X-API-Key: srt_live_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

API Key ขอได้จาก Super Admin ผ่านหน้า **Admin → API Keys**

> **ข้อสำคัญ:** เก็บ API Key เป็นความลับ อย่า commit ลง source code  
> ถ้า Key หาย ให้ Revoke ทันทีและขอ Key ใหม่

---

## Scopes (สิทธิ์)

แต่ละ API Key ถูกกำหนด Scope ที่อนุญาต — request ที่ใช้ Scope นอกเหนือจากที่กำหนดจะได้รับ `403 Forbidden`

| Scope | คำอธิบาย |
|---|---|
| `complaints:read` | ดูรายละเอียดเรื่องร้องเรียน |
| `complaints:write` | สร้างเรื่องร้องเรียนใหม่ |
| `complaints:status` | ดูสถานะเรื่องร้องเรียน |
| `complaints:update` | อัปเดตสถานะเรื่องร้องเรียน |
| `complaints:edoc` | ดึง e-Document payload |
| `stats:summary` | ดูสถิติสรุป |
| `stats:detailed` | ดูสถิติละเอียด |
| `corruption:stats` | ดูสถิติเรื่องทุจริต |
| `webhooks:manage` | จัดการ Webhook endpoints |

---

## Rate Limiting

- **Fixed Window:** นับ request ต่อนาทีต่อ API Key
- ค่าเริ่มต้น: **60 requests/นาที** (ปรับได้ตอนสร้าง Key ใน Admin → API Keys)
- เมื่อเกิน limit → `429 Too Many Requests`

```json
{
  "error": "Rate limit exceeded",
  "retryAfter": "60s"
}
```

---

## Error Responses

รูปแบบ error response มาตรฐาน:

```json
{
  "error": "คำอธิบายข้อผิดพลาด",
  "details": { }
}
```

| HTTP Status | ความหมาย |
|---|---|
| `400 Bad Request` | ข้อมูลที่ส่งมาไม่ถูกต้อง |
| `401 Unauthorized` | ไม่มี API Key หรือ Key ไม่ถูกต้อง |
| `403 Forbidden` | Key ไม่มีสิทธิ์ (Scope ไม่ตรง) หรือ IP ไม่อยู่ใน Whitelist |
| `404 Not Found` | ไม่พบข้อมูล |
| `429 Too Many Requests` | เกิน Rate Limit |
| `500 Internal Server Error` | ข้อผิดพลาดของระบบ |

---

## Complaints API

### GET /api/complaints/{referenceNumber}

ดูรายละเอียดเรื่องร้องเรียนทั้งหมด

**Scope ที่ต้องการ:** `complaints:read`

**ตัวอย่าง Request:**

```http
GET /api/complaints/SRT-COMPL-2568-0001
X-API-Key: srt_live_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

**ตัวอย่าง Response (200 OK):**

```json
{
  "referenceNumber": "SRT-COMPL-2568-0001",
  "status": "InProgress",
  "statusTh": "กำลังดำเนินการ",
  "priority": "Normal",
  "priorityTh": "ปกติ",
  "category": "บริการบนขบวนรถ",
  "department": "ฝ่ายการเดินรถ",
  "reporterName": "สมชาย ใจดี",
  "reporterPhone": "0812345678",
  "reporterEmail": "somchai@example.com",
  "subjectStation": "สถานีกรุงเทพ",
  "incidentDate": "2025-05-20",
  "description": "พนักงานบนรถไฟไม่สุภาพ...",
  "assignedTo": "นายวิชัย รักงาน",
  "slaDeadline": "2025-05-27T09:00:00Z",
  "slaBreached": false,
  "createdAt": "2025-05-20T10:30:00Z",
  "updatedAt": "2025-05-21T08:00:00Z",
  "closedAt": null,
  "satisfactionScore": null
}
```

---

### POST /api/complaints

สร้างเรื่องร้องเรียนใหม่จากระบบภายนอก

**Scope ที่ต้องการ:** `complaints:write`

**Request Body:**

```json
{
  "reporterName": "สมชาย ใจดี",
  "reporterPhone": "0812345678",
  "reporterEmail": "somchai@example.com",
  "categoryId": 3,
  "subjectStation": "สถานีกรุงเทพ",
  "incidentDate": "2025-05-20T00:00:00Z",
  "description": "รายละเอียดเรื่องร้องเรียน (ขั้นต่ำ 10 ตัวอักษร)"
}
```

| Field | Type | Required | คำอธิบาย |
|---|---|---|---|
| `reporterName` | string | ✅ | ชื่อผู้ร้องเรียน (max 200) |
| `reporterPhone` | string | ✅ | เบอร์โทรศัพท์ (max 20) |
| `reporterEmail` | string | — | อีเมล (ตรวจรูปแบบ) |
| `categoryId` | int | ✅ | ID หมวดหมู่เรื่อง (ดูจาก Admin → Categories) |
| `subjectStation` | string | — | สถานี/สถานที่เกิดเหตุ |
| `incidentDate` | datetime | — | วันที่เกิดเหตุ (ISO 8601) |
| `description` | string | ✅ | รายละเอียด (min 10 ตัวอักษร) |

**ตัวอย่าง Response (201 Created):**

```json
{
  "referenceNumber": "SRT-COMPL-2568-0042",
  "status": "Pending",
  "slaDeadline": "2025-05-27T09:00:00Z",
  "trackingUrl": "https://www.railway.co.th/complaint/track/SRT-COMPL-2568-0042",
  "message": "รับเรื่องร้องเรียนเรียบร้อยแล้ว"
}
```

---

### GET /api/complaints/{referenceNumber}/status

ดูเฉพาะสถานะเรื่องร้องเรียน (lightweight endpoint)

**Scope ที่ต้องการ:** `complaints:status`

**ตัวอย่าง Response (200 OK):**

```json
{
  "referenceNumber": "SRT-COMPL-2568-0001",
  "status": "Resolved",
  "statusTh": "แก้ไขแล้ว",
  "updatedAt": "2025-05-25T14:30:00Z",
  "closedAt": "2025-05-25T14:30:00Z",
  "slaBreached": false
}
```

---

### PUT /api/complaints/{referenceNumber}/status

อัปเดตสถานะเรื่องร้องเรียนจากระบบภายนอก

**Scope ที่ต้องการ:** `complaints:update`

**Request Body:**

```json
{
  "newStatus": "Resolved",
  "note": "ดำเนินการแก้ไขเรียบร้อยแล้ว"
}
```

| Field | Type | Required | คำอธิบาย |
|---|---|---|---|
| `newStatus` | string | ✅ | ค่าที่รองรับด้านล่าง |
| `note` | string | — | หมายเหตุแนบการเปลี่ยนสถานะ |

**ค่าสถานะที่รองรับ:**

| ค่า | ความหมาย |
|---|---|
| `Pending` | รอดำเนินการ |
| `InProgress` | กำลังดำเนินการ |
| `WaitingInfo` | รอข้อมูลเพิ่มเติม |
| `Forwarded` | ส่งต่อแผนก |
| `UnderReview` | อยู่ระหว่างพิจารณา |
| `Resolved` | แก้ไขแล้ว |
| `Closed` | ปิดเรื่อง |
| `Rejected` | ปฏิเสธ |

**ตัวอย่าง Response (200 OK):**

```json
{
  "referenceNumber": "SRT-COMPL-2568-0001",
  "newStatus": "Resolved",
  "message": "อัปเดตสถานะเรียบร้อยแล้ว"
}
```

---

### GET /api/complaints/{referenceNumber}/edoc-payload

ดึงข้อมูล payload สำหรับระบบ e-Document ของ รฟท.

**Scope ที่ต้องการ:** `complaints:edoc`

**ตัวอย่าง Response (200 OK):**

```json
{
  "schemaVersion": "1.0",
  "generatedAt": "2025-05-28T07:00:00Z",
  "referenceNumber": "SRT-COMPL-2568-0001",
  "reporter": {
    "name": "สมชาย ใจดี",
    "phone": "0812345678",
    "email": "somchai@example.com"
  },
  "complaint": {
    "category": "บริการบนขบวนรถ",
    "department": "ฝ่ายการเดินรถ",
    "priority": "Normal",
    "priorityTh": "ปกติ",
    "subjectStation": "สถานีกรุงเทพ",
    "incidentDate": "2025-05-20",
    "description": "พนักงานบนรถไฟไม่สุภาพ...",
    "submittedAt": "2025-05-20T10:30:00Z"
  },
  "resolution": {
    "status": "Resolved",
    "statusTh": "แก้ไขแล้ว",
    "assignedTo": "นายวิชัย รักงาน",
    "slaDeadline": "2025-05-27T09:00:00Z",
    "slaBreached": false,
    "closedAt": "2025-05-25T14:30:00Z",
    "resolutionNote": "ได้ดำเนินการตักเตือนพนักงานแล้ว"
  }
}
```

---

## Statistics API

### GET /api/stats/summary

สรุปสถิติเรื่องร้องเรียนทั่วไป

**Scope ที่ต้องการ:** `stats:summary`

**ตัวอย่าง Response (200 OK):**

```json
{
  "asOf": "2025-05-28T07:00:00Z",
  "complaints": {
    "total": 1420,
    "pending": 38,
    "inProgress": 125,
    "resolved": 1180,
    "closed": 60,
    "rejected": 17,
    "slaBreached": 5,
    "todayNew": 12
  }
}
```

---

### GET /api/stats/detailed

สถิติแยกรายหมวด ความเร่งด่วน และสถานะ

**Scope ที่ต้องการ:** `stats:detailed`

**ตัวอย่าง Response (200 OK):**

```json
{
  "asOf": "2025-05-28T07:00:00Z",
  "byCategory": [
    { "category": "บริการบนขบวนรถ", "count": 420 },
    { "category": "ความปลอดภัย", "count": 310 }
  ],
  "byPriority": [
    { "priority": "Critical", "count": 8 },
    { "priority": "Normal", "count": 980 }
  ],
  "byStatus": [
    { "status": "Pending", "count": 38 },
    { "status": "InProgress", "count": 125 }
  ],
  "averageResolutionHours": 54.3
}
```

---

### GET /api/stats/corruption

สรุปสถิติเรื่องแจ้งเบาะแสทุจริต

**Scope ที่ต้องการ:** `corruption:stats`

**ตัวอย่าง Response (200 OK):**

```json
{
  "asOf": "2025-05-28T07:00:00Z",
  "reports": {
    "total": 47,
    "pending": 5,
    "inProgress": 12,
    "underReview": 8,
    "closed": 20,
    "rejected": 2,
    "slaBreached": 1,
    "todayNew": 0
  },
  "bySubjectType": [
    { "subjectType": "Employee", "count": 30 },
    { "subjectType": "Contractor", "count": 12 },
    { "subjectType": "Department", "count": 5 }
  ]
}
```

---

## Webhooks API

ระบบสามารถส่ง event แจ้งเตือนออกไปยัง endpoint ที่ลงทะเบียนไว้ได้แบบ real-time

### GET /api/webhooks

ดูรายการ Webhook ที่ลงทะเบียนด้วย API Key นี้

**Scope ที่ต้องการ:** `webhooks:manage`

**ตัวอย่าง Response (200 OK):**

```json
[
  {
    "id": 1,
    "name": "สถานะเรื่องร้องเรียน",
    "targetUrl": "https://your-system.example.com/webhook/srt",
    "isActive": true,
    "createdAt": "2025-05-01T09:00:00Z",
    "lastTriggeredAt": "2025-05-28T06:30:00Z",
    "lastStatusCode": 200,
    "events": ["complaint.status_changed", "complaint.closed"]
  }
]
```

---

### POST /api/webhooks

ลงทะเบียน Webhook endpoint ใหม่

**Scope ที่ต้องการ:** `webhooks:manage`

**Request Body:**

```json
{
  "name": "ชื่อ webhook (สำหรับอ้างอิง)",
  "targetUrl": "https://your-system.example.com/webhook/srt",
  "events": ["complaint.created", "complaint.status_changed"]
}
```

| Field | Type | Required | คำอธิบาย |
|---|---|---|---|
| `name` | string | ✅ | ชื่อ webhook (max 200) |
| `targetUrl` | string | ✅ | URL ที่รับ event (ต้องเป็น HTTPS) |
| `events` | string[] | ✅ | รายการ events ที่ต้องการรับ (ดูรายการด้านล่าง) |

**ตัวอย่าง Response (201 Created):**

```json
{
  "id": 2,
  "name": "ชื่อ webhook",
  "targetUrl": "https://your-system.example.com/webhook/srt",
  "isActive": true,
  "createdAt": "2025-05-28T07:00:00Z",
  "events": ["complaint.created"],
  "secret": "whsec_a1b2c3d4e5f6...",
  "message": "เก็บ secret นี้ไว้ใช้ยืนยัน signature ของ webhook — จะไม่แสดงอีกครั้ง"
}
```

> **สำคัญ:** `secret` จะแสดงครั้งเดียวเท่านั้น — เก็บไว้ใช้ verify HMAC-SHA256 signature

---

### DELETE /api/webhooks/{id}

ลบ Webhook

**Scope ที่ต้องการ:** `webhooks:manage`

```http
DELETE /api/webhooks/2
X-API-Key: srt_live_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

**Response (200 OK):**

```json
{
  "message": "ลบ webhook เรียบร้อยแล้ว"
}
```

---

## Webhook Events (Outbound)

เมื่อ event เกิดขึ้น ระบบจะส่ง HTTP POST ไปยัง `targetUrl` ที่ลงทะเบียนไว้

### Request Headers ที่ส่งไป

```
Content-Type: application/json
X-SRT-Event: complaint.status_changed
X-SRT-Signature: sha256=abcdef1234567890...
X-SRT-Timestamp: 1717123456
```

### Verify Signature

```python
import hmac, hashlib

def verify(secret: str, payload: bytes, signature_header: str) -> bool:
    expected = "sha256=" + hmac.new(
        secret.encode(), payload, hashlib.sha256
    ).hexdigest()
    return hmac.compare_digest(expected, signature_header)
```

```csharp
// C#
using System.Security.Cryptography;
using System.Text;

bool Verify(string secret, byte[] payload, string signatureHeader)
{
    var key = Encoding.UTF8.GetBytes(secret);
    using var hmac = new HMACSHA256(key);
    var hash = "sha256=" + Convert.ToHexString(hmac.ComputeHash(payload)).ToLower();
    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(hash),
        Encoding.UTF8.GetBytes(signatureHeader));
}
```

---

> **หมายเหตุ:** Body เป็น payload โดยตรง ไม่มี wrapper object

### Event: `complaint.created`

Header: `X-SRT-Event: complaint.created`

```json
{
  "referenceNumber": "SRT-COMPL-2568-0042",
  "status": "Pending",
  "priority": "Normal",
  "createdAt": "2025-05-28T07:00:00Z"
}
```

---

### Event: `complaint.status_changed`

Header: `X-SRT-Event: complaint.status_changed`

```json
{
  "referenceNumber": "SRT-COMPL-2568-0042",
  "oldStatus": "Pending",
  "newStatus": "InProgress",
  "updatedAt": "2025-05-28T10:00:00Z"
}
```

---

### Event: `complaint.closed`

Header: `X-SRT-Event: complaint.closed`

```json
{
  "referenceNumber": "SRT-COMPL-2568-0042",
  "oldStatus": "InProgress",
  "newStatus": "Resolved",
  "updatedAt": "2025-05-28T15:00:00Z"
}
```

### Retry Policy

ระบบลอง retry อัตโนมัติหาก endpoint ตอบกลับ status ≥ 400 หรือ timeout (สูงสุด 4 ครั้ง):

| ครั้งที่ | รอนาน |
|---|---|
| 1 | 5 นาที |
| 2 | 30 นาที |
| 3 | 2 ชั่วโมง |
| 4 | ไม่ retry อีก |

---

## ตัวอย่างการใช้งานด้วย cURL

```bash
# ดูรายละเอียดเรื่องร้องเรียน
curl -H "X-API-Key: srt_live_xxx" \
  https://www.railway.co.th/complaint/api/complaints/SRT-COMPL-2568-0001

# สร้างเรื่องร้องเรียนใหม่
curl -X POST \
  -H "X-API-Key: srt_live_xxx" \
  -H "Content-Type: application/json" \
  -d '{"reporterName":"สมชาย ใจดี","reporterPhone":"0812345678","categoryId":3,"description":"รายละเอียดเรื่องร้องเรียน"}' \
  https://www.railway.co.th/complaint/api/complaints

# อัปเดตสถานะ
curl -X PUT \
  -H "X-API-Key: srt_live_xxx" \
  -H "Content-Type: application/json" \
  -d '{"newStatus":"Resolved","note":"ดำเนินการแล้วเสร็จ"}' \
  https://www.railway.co.th/complaint/api/complaints/SRT-COMPL-2568-0001/status

# ดูสถิติสรุป
curl -H "X-API-Key: srt_live_xxx" \
  https://www.railway.co.th/complaint/api/stats/summary
```

---

## Traffy Fondue Webhook Receiver

Endpoint สำหรับรับ push notification จาก **Traffy Fondue Exchange API** ของ NECTEC — **ไม่ต้องใช้ `X-API-Key`** แต่ใช้ `{secret}` ใน URL path แทน

> ตั้งค่า secret ได้ที่ **Admin → ตั้งค่าระบบ → Traffy Fondue → Webhook Secret**  
> แล้วแจ้ง URL ทั้งสองด้านล่างให้ทีม NECTEC ลงทะเบียน

---

### POST /api/traffy-webhook/{secret}/new-issue

รับเรื่องใหม่จาก Traffy แล้ว import เข้าระบบทันที (dedup ด้วย `ticket_id` อัตโนมัติ)

**Path Parameter:** `{secret}` — ตรงกับที่ตั้งใน Admin

**Request Body (Traffy format):**

```json
{
  "ticket_id": "traffy_abc123",
  "description": "ถนนเสียหายข้างสถานีรถไฟ",
  "address": "ถนนสุขุมวิท กรุงเทพ",
  "type": "ถนน",
  "topic": ["โครงสร้างพื้นฐาน"],
  "timestamp": "2026-06-01T09:00:00Z",
  "status": "รอรับเรื่อง",
  "name": "สมหญิง ดีใจ",
  "phone": "0898765432"
}
```

| Field | Type | Required | คำอธิบาย |
|---|---|---|---|
| `ticket_id` | string | ✅ | รหัสเรื่องใน Traffy (ใช้ dedup) |
| `description` | string | — | รายละเอียดเรื่อง |
| `address` | string | — | สถานที่เกิดเหตุ → บันทึกเป็น `SubjectStation` |
| `type` | string | — | ประเภทเรื่อง (Traffy category) |
| `topic` | string[] | — | หมวดย่อย |
| `timestamp` | ISO 8601 | — | วันที่แจ้งเรื่อง |
| `status` | string | — | สถานะใน Traffy |
| `name` | string | — | ชื่อผู้แจ้ง |
| `phone` | string | — | เบอร์โทร (ระบบ normalize ให้อัตโนมัติ — รับ 9–10 หลัก) |

**Response `200 OK` — เรื่องใหม่ (import สำเร็จ):**

```json
{
  "imported": true,
  "reference": "SRT-COMPL-2026-0042"
}
```

**Response `200 OK` — เรื่องซ้ำ (ข้าม):**

```json
{
  "imported": false,
  "reason": "duplicate"
}
```

**Response `401 Unauthorized` — secret ผิด:**

ไม่มี body

---

### PATCH /api/traffy-webhook/{secret}/update-status

รับอัปเดตสถานะจาก Traffy แล้ว sync เข้าระบบ รฟท.

**Path Parameter:** `{secret}` — ตรงกับที่ตั้งใน Admin

**Request Body:**

```json
{
  "ticket_id": "traffy_abc123",
  "status_id": 3,
  "note": "ดำเนินการแก้ไขเสร็จสิ้น"
}
```

| Field | Type | Required | คำอธิบาย |
|---|---|---|---|
| `ticket_id` | string | ✅ | รหัสเรื่องใน Traffy |
| `status_id` | integer | ✅ | สถานะ Traffy (ดู mapping ด้านล่าง) |
| `note` | string | — | หมายเหตุจาก Traffy (บันทึกใน complaint notes) |

**Traffy `status_id` → สถานะใน รฟท.:**

| `status_id` | สถานะ รฟท. | คำอธิบาย |
|---|---|---|
| `3` | `Resolved` | แก้ไขแล้ว |
| `4` | `Rejected` | ปฏิเสธ |
| อื่นๆ | *(ไม่เปลี่ยน)* | ระบบบันทึก log แต่ไม่ sync สถานะ |

**Response `200 OK`:**

```json
{
  "updated": true
}
```

**Response `200 OK` — ไม่พบ ticket ในระบบ รฟท.:**

```json
{
  "updated": false,
  "reason": "not_found"
}
```

---

## Reference Number Format

| Track | Format | ตัวอย่าง |
|---|---|---|
| เรื่องร้องเรียนทั่วไป | `SRT-COMPL-{ปีพ.ศ.}-{seq:D4}` | `SRT-COMPL-2568-0042` |
| เรื่องทุจริต | `SRT-CORUPT-{ปีพ.ศ.}-{seq:D4}` | `SRT-CORUPT-2568-0005` |

ลำดับ (seq) วิ่งต่อเนื่องไม่ reset ตามปี
