# Design Component

สร้าง Tailwind CSS component ใหม่ตาม SRT Design System

## Usage

```
/design-component ComponentType ComponentName [context]
```

ตัวอย่าง:
- `/design-component card CaseDetailCard`
- `/design-component modal ConfirmCloseModal`
- `/design-component sidebar StaffSidebar`
- `/design-component stat-grid DashboardStats`
- `/design-component timeline CaseTimeline`

## ComponentType ที่รองรับ

- `card` — Card แสดงข้อมูล
- `modal` — Dialog / Popup
- `sidebar` — Navigation sidebar
- `stat-grid` — กล่อง metric / stat สำหรับ dashboard
- `timeline` — Timeline ความคืบหน้า
- `table` — Data table พร้อม filter
- `empty-state` — หน้าว่าง / ไม่มีข้อมูล
- `page-header` — Header ของแต่ละหน้า
- `toast` — Notification toast

## SRT Design System Reference

### สี

```
srt-navy:       #0D2E6E  (Primary — backgrounds, buttons หลัก)
srt-navy-dark:  #071A40  (Hover state ของ navy)
srt-navy-light: #1A4A9A  (Secondary navy)
srt-gold:       #F59E0B  (Accent — CTA, highlight)
```

### Status Badge Classes

```html
<!-- รอรับเรื่อง -->
<span class="px-2 py-0.5 rounded-full text-xs font-medium bg-amber-100 text-amber-800">
  รอรับเรื่อง
</span>

<!-- กำลังดำเนินการ -->
<span class="px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
  กำลังดำเนินการ
</span>

<!-- ปิดเรื่อง -->
<span class="px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
  ปิดเรื่อง
</span>

<!-- เร่งด่วน -->
<span class="px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
  เร่งด่วน
</span>

<!-- ทุจริต -->
<span class="px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
  ทุจริต
</span>
```

### Button Classes

```html
<!-- Primary -->
<button class="bg-srt-navy text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-srt-navy-dark transition-colors">
  บันทึก
</button>

<!-- Secondary / Outline -->
<button class="border border-srt-navy text-srt-navy px-4 py-2 rounded-md text-sm font-medium hover:bg-srt-navy/5 transition-colors">
  ยกเลิก
</button>

<!-- Danger -->
<button class="bg-red-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-red-700 transition-colors">
  ปฏิเสธเรื่อง
</button>

<!-- Ghost / Text -->
<button class="text-srt-navy text-sm font-medium hover:underline">
  ดูรายละเอียด
</button>
```

### Card Base

```html
<div class="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
  <!-- content -->
</div>
```

### Input Base

```html
<input type="text"
  class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm
         focus:outline-none focus:border-srt-navy focus:ring-2 focus:ring-srt-navy/10
         transition-colors">
```

### Sidebar

```html
<aside class="bg-srt-navy min-h-screen w-60 p-4">
  <!-- Nav item active -->
  <a class="flex items-center gap-2 px-3 py-2 rounded-md text-sm
            bg-white/15 text-white font-medium">
    Dashboard
  </a>
  <!-- Nav item default -->
  <a class="flex items-center gap-2 px-3 py-2 rounded-md text-sm
            text-white/70 hover:bg-white/10 hover:text-white transition-colors">
    คิวเรื่อง
  </a>
</aside>
```

### Font

```html
<!-- ใน _Layout.cshtml ต้องมี -->
<link href="https://fonts.googleapis.com/css2?family=Sarabun:wght@300;400;500;600&display=swap" rel="stylesheet">
```

```css
/* tailwind.config.js */
fontFamily: {
  sans: ['Sarabun', 'sans-serif'],
}
```

## Steps

1. วิเคราะห์ ComponentType และ context ที่ต้องการ
2. เขียน HTML + Tailwind classes ตาม SRT Design System
3. ใช้ HTMX attributes ถ้า component มี interaction (เช่น modal ใช้ hx-target)
4. เพิ่ม Razor syntax (`@Model`, `@if`, `@foreach`) ตาม component
5. เพิ่ม Partial View (`_ComponentName.cshtml`) พร้อม `@model` declaration
6. แสดง usage example ว่าเรียกใช้ยังไง
7. ตรวจสอบ Thai font ว่าใช้ Sarabun ถูกต้อง

## Output Format

สร้างไฟล์ `Pages/Shared/_ComponentName.cshtml` พร้อม:
- HTML + Tailwind
- Razor syntax
- HTMX attributes (ถ้ามี)
- ตัวอย่างการเรียกใช้ใน parent page
