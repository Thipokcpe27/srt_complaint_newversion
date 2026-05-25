# Design Form

สร้าง Form สำหรับ Razor Pages พร้อม validation, error state, และ HTMX

## Usage

```
/design-form FormName [fields...]
```

ตัวอย่าง:
- `/design-form ComplaintSubmit name,phone,email,category,description,files`
- `/design-form StaffLogin employeeCode,password`
- `/design-form SlaSettings priority,hours,autoAssign`

## Steps

1. วิเคราะห์ fields ที่ต้องการ (ประเภท, required, validation)
2. สร้าง ViewModel class สำหรับ binding
3. สร้าง Razor Page form พร้อม:
   - Anti-forgery token
   - Label + input ทุก field
   - Required indicator (*)
   - Error message ด้วย `asp-validation-for`
   - Submit button
4. เพิ่ม HTMX ถ้าต้องการ submit แบบ AJAX
5. เพิ่ม client-side validation summary
6. ตรวจ accessibility (label for, aria-required, aria-describedby)

## Field Types

```html
<!-- Text Input -->
<div class="mb-4">
  <label asp-for="Name" class="block text-sm font-medium text-gray-700 mb-1">
    ชื่อ-นามสกุล <span class="text-red-500" aria-hidden="true">*</span>
  </label>
  <input asp-for="Name" type="text"
    class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm
           focus:outline-none focus:border-srt-navy focus:ring-2 focus:ring-srt-navy/10
           transition-colors">
  <span asp-validation-for="Name"
    class="text-red-600 text-xs mt-1 block"></span>
</div>

<!-- Select / Dropdown -->
<div class="mb-4">
  <label asp-for="CategoryId" class="block text-sm font-medium text-gray-700 mb-1">
    ประเภทเรื่อง <span class="text-red-500" aria-hidden="true">*</span>
  </label>
  <select asp-for="CategoryId" asp-items="Model.CategoryOptions"
    class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm
           focus:outline-none focus:border-srt-navy focus:ring-2 focus:ring-srt-navy/10">
    <option value="">-- เลือกประเภท --</option>
  </select>
  <span asp-validation-for="CategoryId" class="text-red-600 text-xs mt-1 block"></span>
</div>

<!-- Textarea -->
<div class="mb-4">
  <label asp-for="Description" class="block text-sm font-medium text-gray-700 mb-1">
    รายละเอียด <span class="text-red-500" aria-hidden="true">*</span>
  </label>
  <textarea asp-for="Description" rows="5"
    class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm resize-none
           focus:outline-none focus:border-srt-navy focus:ring-2 focus:ring-srt-navy/10"
    placeholder="อธิบายรายละเอียดเรื่องที่ต้องการร้องเรียน..."></textarea>
  <span asp-validation-for="Description" class="text-red-600 text-xs mt-1 block"></span>
</div>

<!-- File Upload -->
<div class="mb-4">
  <label class="block text-sm font-medium text-gray-700 mb-1">
    ไฟล์แนบ <span class="text-gray-400 font-normal">(ไม่เกิน 5 ไฟล์, 10MB ต่อไฟล์)</span>
  </label>
  <div class="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center
              hover:border-srt-navy transition-colors cursor-pointer">
    <i class="ti ti-upload text-2xl text-gray-400 mb-2 block" aria-hidden="true"></i>
    <p class="text-sm text-gray-500">ลากไฟล์มาวาง หรือ</p>
    <label class="mt-2 inline-block cursor-pointer">
      <span class="text-srt-navy text-sm font-medium hover:underline">คลิกเพื่อเลือกไฟล์</span>
      <input type="file" name="Attachments" multiple accept=".jpg,.png,.pdf,.doc,.docx"
        class="sr-only">
    </label>
    <p class="text-xs text-gray-400 mt-1">JPG, PNG, PDF, DOC, DOCX</p>
  </div>
</div>

<!-- Radio Group -->
<div class="mb-4">
  <p class="block text-sm font-medium text-gray-700 mb-2">ความเร่งด่วน</p>
  <div class="flex flex-col gap-2">
    @foreach (var option in Model.PriorityOptions)
    {
      <label class="flex items-center gap-2 cursor-pointer">
        <input type="radio" asp-for="Priority" value="@option.Value"
          class="w-4 h-4 text-srt-navy border-gray-300 focus:ring-srt-navy">
        <span class="text-sm text-gray-700">@option.Text</span>
      </label>
    }
  </div>
</div>

<!-- Date Picker -->
<div class="mb-4">
  <label asp-for="IncidentDate" class="block text-sm font-medium text-gray-700 mb-1">
    วันที่เกิดเหตุ
  </label>
  <input asp-for="IncidentDate" type="date"
    class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm
           focus:outline-none focus:border-srt-navy focus:ring-2 focus:ring-srt-navy/10">
</div>
```

## Form Wrapper + Submit

```html
<form method="post" enctype="multipart/form-data"
  hx-post="/complaint/submit"
  hx-target="#form-result"
  hx-indicator="#submit-spinner">

  @Html.AntiForgeryToken()

  <div asp-validation-summary="ModelOnly"
    class="mb-4 p-3 bg-red-50 border border-red-200 rounded-md text-sm text-red-700
           [&:empty]:hidden">
  </div>

  <!-- fields here -->

  <div class="flex items-center gap-3 mt-6">
    <button type="submit"
      class="bg-srt-navy text-white px-6 py-2.5 rounded-md text-sm font-medium
             hover:bg-srt-navy-dark transition-colors
             disabled:opacity-50 disabled:cursor-not-allowed">
      <span id="submit-spinner"
        class="htmx-indicator inline-block w-4 h-4 border-2 border-white/30
               border-t-white rounded-full animate-spin mr-2"></span>
      ยื่นเรื่องร้องเรียน
    </button>
    <a href="/" class="text-sm text-gray-500 hover:text-gray-700">ยกเลิก</a>
  </div>
</form>

<div id="form-result"></div>
```

## ViewModel Template

```csharp
public class ComplaintSubmitViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
    [Phone(ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกประเภทเรื่อง")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "กรุณากรอกรายละเอียด")]
    [MinLength(20, ErrorMessage = "กรุณากรอกรายละเอียดอย่างน้อย 20 ตัวอักษร")]
    public string Description { get; set; } = string.Empty;

    public List<IFormFile> Attachments { get; set; } = new();

    // SelectList สำหรับ Dropdown
    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = [];
}
```
