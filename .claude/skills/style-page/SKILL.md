# Style Page

จัด style หน้า Razor Pages ที่มีอยู่แล้ว ให้ตรงตาม SRT Design System

## Usage

```
/style-page PagePath [focus]
```

ตัวอย่าง:
- `/style-page Pages/Public/Submit.cshtml`
- `/style-page Pages/Staff/Dashboard.cshtml focus=layout`
- `/style-page Pages/Admin/Users.cshtml focus=table`

## Focus Options

- `layout` — จัดวาง layout, spacing, responsive
- `form` — style form elements
- `table` — style data table
- `typography` — ขนาด font, สี, hierarchy
- `colors` — ปรับ color ให้ตรง SRT palette
- `all` — ปรับทุกอย่าง (default)

## Steps

1. อ่านไฟล์ Razor Page ที่ระบุ
2. วิเคราะห์ปัญหา: spacing, colors, typography, layout
3. ปรับ Tailwind classes ให้ตรง SRT Design System:
   - Primary color → `bg-srt-navy`, `text-srt-navy`
   - Font → `font-sans` (Sarabun)
   - Spacing → ใช้ Tailwind scale ที่สม่ำเสมอ
   - Border radius → `rounded-md` (8px) หรือ `rounded-lg` (10px)
4. ตรวจ responsive: mobile (`sm:`) → tablet (`md:`) → desktop (`lg:`)
5. ตรวจ HTMX attributes ยังทำงานได้หลังแก้ style
6. แสดง diff ก่อน/หลัง พร้อมอธิบายที่เปลี่ยน

## SRT Layout Patterns

### หน้า Staff / Admin (มี Sidebar)

```html
<div class="flex min-h-screen bg-gray-50">
  <!-- Sidebar -->
  <aside class="w-60 bg-srt-navy flex-shrink-0">
    @await Html.PartialAsync("_StaffSidebar")
  </aside>

  <!-- Main Content -->
  <main class="flex-1 overflow-auto">
    <!-- Page Header -->
    <div class="bg-white border-b border-gray-200 px-6 py-4">
      <h1 class="text-lg font-semibold text-gray-900">ชื่อหน้า</h1>
      <p class="text-sm text-gray-500 mt-0.5">คำอธิบาย</p>
    </div>

    <!-- Content -->
    <div class="p-6">
      <!-- content here -->
    </div>
  </main>
</div>
```

### หน้า Public (ไม่มี Sidebar)

```html
<!-- Topbar -->
<header class="bg-srt-navy text-white">
  <div class="max-w-4xl mx-auto px-4 py-4 flex items-center gap-3">
    <img src="/images/srt-logo.svg" class="h-8" alt="SRT">
    <span class="font-medium">ระบบรับเรื่องร้องเรียน</span>
  </div>
</header>

<!-- Content -->
<main class="max-w-4xl mx-auto px-4 py-8">
  <!-- content here -->
</main>
```

### Section / Card Container

```html
<div class="bg-white rounded-lg border border-gray-200 shadow-sm">
  <div class="px-5 py-4 border-b border-gray-100">
    <h2 class="text-base font-semibold text-gray-900">หัวข้อ Section</h2>
  </div>
  <div class="p-5">
    <!-- content -->
  </div>
</div>
```

## Spacing Scale ที่ใช้

```
p-4   = 16px  (card padding เล็ก)
p-5   = 20px  (card padding กลาง)
p-6   = 24px  (page padding)
gap-4 = 16px  (gap ระหว่าง elements)
gap-6 = 24px  (gap ระหว่าง sections)
mb-6  = 24px  (margin ระหว่าง sections)
```

## Typography Scale

```
text-xs   = 12px  (hint, caption)
text-sm   = 14px  (body, labels)
text-base = 16px  (body หลัก)
text-lg   = 18px  (page title)
text-xl   = 20px  (section heading ใหญ่)

font-normal  = 400
font-medium  = 500  (labels, subheadings)
font-semibold = 600 (headings)
```
