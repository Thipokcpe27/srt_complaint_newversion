# Check Accessibility (a11y)

ตรวจสอบ accessibility ของ Razor Pages / HTML components

## Usage

```
/check-a11y [FilePath]
```

ตัวอย่าง:
- `/check-a11y Pages/Public/Submit.cshtml`
- `/check-a11y Pages/Shared/_CaseCard.cshtml`
- `/check-a11y` (ตรวจทั้งโปรเจกต์ — สุ่มตัวอย่าง 5 ไฟล์)

## Steps

1. อ่านไฟล์ที่ระบุ
2. ตรวจสอบตาม checklist ด้านล่าง
3. แสดง issues แต่ละอัน พร้อม severity (Critical / Warning / Info)
4. แสดง code ที่แก้ไขแล้วสำหรับทุก issue
5. สรุปคะแนน a11y โดยรวม

## Checklist

### 1. Images & Icons (Critical)

```html
<!-- ❌ ขาด alt -->
<img src="/logo.png">

<!-- ✅ มี alt ที่สื่อความหมาย -->
<img src="/logo.png" alt="โลโก้การรถไฟแห่งประเทศไทย">

<!-- ✅ icon ที่เป็น decorative ใช้ aria-hidden -->
<i class="ti ti-bell" aria-hidden="true"></i>

<!-- ✅ icon ที่มี action ใช้ aria-label -->
<button aria-label="แจ้งเตือน">
  <i class="ti ti-bell" aria-hidden="true"></i>
</button>
```

### 2. Form Labels (Critical)

```html
<!-- ❌ ขาด label -->
<input type="text" placeholder="ชื่อ">

<!-- ✅ มี label ที่ผูกกับ input -->
<label for="reporter-name" class="text-sm font-medium text-gray-700">
  ชื่อ-นามสกุล <span class="text-red-500" aria-hidden="true">*</span>
  <span class="sr-only">(จำเป็น)</span>
</label>
<input id="reporter-name" type="text" aria-required="true">
```

### 3. Buttons & Links (Critical)

```html
<!-- ❌ button ไม่มีข้อความ -->
<button><i class="ti ti-edit"></i></button>

<!-- ✅ มี aria-label -->
<button aria-label="แก้ไขเรื่องร้องเรียน">
  <i class="ti ti-edit" aria-hidden="true"></i>
</button>

<!-- ❌ link ไม่มีความหมาย -->
<a href="/case/1">คลิกที่นี่</a>

<!-- ✅ link มีความหมาย -->
<a href="/case/1">ดูรายละเอียดเรื่อง GEN-2568-00142</a>
```

### 4. Color Contrast (Critical)

```
ต้องผ่าน WCAG AA:
- Normal text: contrast ratio ≥ 4.5:1
- Large text (18px+ / bold 14px+): ≥ 3:1

SRT Navy (#0D2E6E) บน white → ✅ 10.5:1
Gold (#F59E0B) บน white → ❌ 2.6:1 (ใช้เป็น bg เท่านั้น ไม่ใช่ text)
Gray-500 (#6B7280) บน white → ✅ 4.6:1
Gray-400 (#9CA3AF) บน white → ❌ 2.6:1 (ต่ำเกินไป)
```

### 5. Error Messages (Warning)

```html
<!-- ❌ error ไม่ผูกกับ input -->
<input type="text" class="border-red-500">
<p class="text-red-500">กรุณากรอกข้อมูล</p>

<!-- ✅ ผูกด้วย aria-describedby -->
<input type="text"
  aria-invalid="true"
  aria-describedby="name-error"
  class="border-red-500">
<p id="name-error" role="alert" class="text-red-600 text-sm mt-1">
  กรุณากรอกชื่อ-นามสกุล
</p>
```

### 6. Focus Management (Warning)

```html
<!-- ✅ Focus ring ต้องมองเห็น -->
<input class="focus:outline-none focus:ring-2 focus:ring-srt-navy focus:ring-offset-1">

<!-- ✅ Modal ต้อง focus เมื่อเปิด -->
<div role="dialog" aria-modal="true" aria-labelledby="modal-title">
  <h2 id="modal-title">ยืนยันปิดเรื่อง</h2>
</div>
```

### 7. Skip Navigation (Info)

```html
<!-- ✅ เพิ่มใน _Layout.cshtml — ด้านบนสุด -->
<a href="#main-content"
   class="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2
          bg-srt-navy text-white px-4 py-2 rounded z-50">
  ข้ามไปยังเนื้อหาหลัก
</a>

<main id="main-content">
  ...
</main>
```

### 8. ARIA Landmarks (Info)

```html
<!-- ✅ ใช้ semantic HTML / ARIA roles -->
<header>...</header>
<nav aria-label="เมนูหลัก">...</nav>
<main>...</main>
<aside aria-label="ข้อมูลสรุป">...</aside>
<footer>...</footer>
```

### 9. Loading States (Info)

```html
<!-- ✅ HTMX loading indicator accessible -->
<div id="main-content"
  hx-indicator="#loading-spinner">
  ...
</div>

<div id="loading-spinner"
  class="htmx-indicator"
  role="status"
  aria-live="polite"
  aria-label="กำลังโหลด...">
  <span class="sr-only">กำลังโหลด</span>
  <!-- spinner icon -->
</div>
```

## Severity Levels

- **Critical** — ต้องแก้ก่อน deploy (มีผลต่อ screen reader)
- **Warning** — ควรแก้ (กระทบ UX บางกลุ่ม)
- **Info** — แนะนำ (best practice)
