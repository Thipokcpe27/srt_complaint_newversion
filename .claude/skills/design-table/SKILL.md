# Design Table

สร้าง Data Table สำหรับ Razor Pages พร้อม sorting, filtering, pagination, และ HTMX

## Usage

```
/design-table TableName [columns...] [features...]
```

ตัวอย่าง:
- `/design-table ComplaintQueue ref,category,reporter,priority,status,date,action`
- `/design-table AdminUserList code,name,role,status,lastLogin,action with-search,with-filter`
- `/design-table ApiRequestLog method,endpoint,status,ip,time`

## Steps

1. วิเคราะห์ columns ที่ต้องการ
2. สร้าง table HTML + Tailwind
3. เพิ่ม HTMX สำหรับ search/filter แบบ real-time (ถ้าต้องการ)
4. เพิ่ม pagination
5. เพิ่ม empty state
6. เพิ่ม loading skeleton
7. สร้าง PageModel handler (`OnGetAsync`) สำหรับ query + filter
8. ตรวจ responsive (scroll horizontal บน mobile)

## Table Template

```html
<!-- Search + Filter Bar -->
<div class="flex items-center gap-3 mb-4">
  <div class="relative flex-1 max-w-xs">
    <i class="ti ti-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
       aria-hidden="true"></i>
    <input type="search"
      placeholder="ค้นหาเลขอ้างอิง, ชื่อ..."
      class="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-md
             focus:outline-none focus:border-srt-navy focus:ring-2 focus:ring-srt-navy/10"
      hx-get="/staff/queue"
      hx-trigger="keyup changed delay:400ms"
      hx-target="#table-body"
      hx-include="[name='status-filter']"
      name="search">
  </div>

  <select name="status-filter"
    class="border border-gray-300 rounded-md px-3 py-2 text-sm
           focus:outline-none focus:border-srt-navy"
    hx-get="/staff/queue"
    hx-trigger="change"
    hx-target="#table-body"
    hx-include="[name='search']">
    <option value="">ทุกสถานะ</option>
    <option value="Pending">รอรับเรื่อง</option>
    <option value="InProgress">กำลังดำเนินการ</option>
    <option value="Closed">ปิดเรื่อง</option>
  </select>

  <span class="text-sm text-gray-500 ml-auto">
    ทั้งหมด <strong class="text-gray-900">@Model.TotalCount</strong> รายการ
  </span>
</div>

<!-- Table -->
<div class="overflow-x-auto rounded-lg border border-gray-200">
  <table class="w-full text-sm border-collapse">
    <thead>
      <tr class="bg-gray-50 text-gray-600 text-left">
        <th class="px-4 py-3 font-medium border-b border-gray-200 whitespace-nowrap">
          เลขที่อ้างอิง
        </th>
        <th class="px-4 py-3 font-medium border-b border-gray-200 whitespace-nowrap">
          ประเภท
        </th>
        <th class="px-4 py-3 font-medium border-b border-gray-200 whitespace-nowrap">
          ผู้ร้อง
        </th>
        <th class="px-4 py-3 font-medium border-b border-gray-200 whitespace-nowrap">
          ความเร่งด่วน
        </th>
        <th class="px-4 py-3 font-medium border-b border-gray-200 whitespace-nowrap">
          สถานะ
        </th>
        <th class="px-4 py-3 font-medium border-b border-gray-200 whitespace-nowrap">
          วันที่รับ
        </th>
        <th class="px-4 py-3 font-medium border-b border-gray-200 whitespace-nowrap">
          SLA
        </th>
        <th class="px-4 py-3 font-medium border-b border-gray-200"></th>
      </tr>
    </thead>
    <tbody id="table-body">
      @if (!Model.Complaints.Any())
      {
        <!-- Empty State -->
        <tr>
          <td colspan="8" class="px-4 py-16 text-center">
            <i class="ti ti-inbox text-4xl text-gray-300 block mb-2" aria-hidden="true"></i>
            <p class="text-gray-400 text-sm">ไม่มีเรื่องร้องเรียนในขณะนี้</p>
          </td>
        </tr>
      }
      @foreach (var item in Model.Complaints)
      {
        <tr class="hover:bg-gray-50 transition-colors border-b border-gray-100 last:border-b-0">
          <td class="px-4 py-3">
            <a href="/staff/case/@item.Id"
              class="text-srt-navy font-medium hover:underline font-mono text-xs">
              @item.ReferenceNumber
            </a>
          </td>
          <td class="px-4 py-3 text-gray-700">@item.CategoryName</td>
          <td class="px-4 py-3 text-gray-700">@item.ReporterName</td>
          <td class="px-4 py-3">
            @* Priority Badge *@
            <span class="px-2 py-0.5 rounded-full text-xs font-medium
              @(item.Priority == "Critical" ? "bg-red-100 text-red-800" :
                item.Priority == "Urgent"   ? "bg-orange-100 text-orange-800" :
                item.Priority == "High"     ? "bg-yellow-100 text-yellow-800" :
                                              "bg-gray-100 text-gray-700")">
              @item.PriorityLabel
            </span>
          </td>
          <td class="px-4 py-3">
            @* Status Badge *@
            <span class="px-2 py-0.5 rounded-full text-xs font-medium
              @(item.Status == "Pending"    ? "bg-amber-100 text-amber-800" :
                item.Status == "InProgress" ? "bg-blue-100 text-blue-800" :
                item.Status == "Closed"     ? "bg-green-100 text-green-800" :
                item.Status == "Rejected"   ? "bg-red-100 text-red-800" :
                                              "bg-gray-100 text-gray-700")">
              @item.StatusLabel
            </span>
          </td>
          <td class="px-4 py-3 text-gray-500 text-xs whitespace-nowrap">
            @item.CreatedAt.ToString("dd MMM yy")
          </td>
          <td class="px-4 py-3">
            @* SLA Countdown *@
            @if (item.SlaBreached)
            {
              <span class="text-red-600 text-xs font-medium flex items-center gap-1">
                <i class="ti ti-alert-circle" aria-hidden="true"></i> เกิน SLA
              </span>
            }
            else if (item.SlaWarning)
            {
              <span class="text-amber-600 text-xs font-medium flex items-center gap-1">
                <i class="ti ti-clock" aria-hidden="true"></i> @item.SlaRemainingText
              </span>
            }
            else
            {
              <span class="text-gray-400 text-xs">@item.SlaRemainingText</span>
            }
          </td>
          <td class="px-4 py-3 text-right">
            @if (item.AssignedToId == null)
            {
              <button
                hx-post="/staff/claim/@item.Id"
                hx-target="closest tr"
                hx-swap="outerHTML"
                class="text-xs bg-srt-navy text-white px-3 py-1.5 rounded hover:bg-srt-navy-dark transition-colors">
                รับเรื่อง
              </button>
            }
            else
            {
              <a href="/staff/case/@item.Id"
                class="text-xs text-srt-navy hover:underline font-medium">
                ดูรายละเอียด →
              </a>
            }
          </td>
        </tr>
      }
    </tbody>
  </table>
</div>

<!-- Pagination -->
@if (Model.TotalPages > 1)
{
  <div class="flex items-center justify-between mt-4">
    <p class="text-sm text-gray-500">
      แสดง @Model.PageStart–@Model.PageEnd จาก @Model.TotalCount รายการ
    </p>
    <div class="flex items-center gap-1">
      @if (Model.CurrentPage > 1)
      {
        <a href="?page=@(Model.CurrentPage - 1)&search=@Model.Search"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50">
          ← ก่อนหน้า
        </a>
      }
      @for (int p = 1; p <= Model.TotalPages; p++)
      {
        <a href="?page=@p&search=@Model.Search"
          class="px-3 py-1.5 text-sm rounded
            @(p == Model.CurrentPage
              ? "bg-srt-navy text-white"
              : "border border-gray-300 hover:bg-gray-50")">
          @p
        </a>
      }
      @if (Model.CurrentPage < Model.TotalPages)
      {
        <a href="?page=@(Model.CurrentPage + 1)&search=@Model.Search"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50">
          ถัดไป →
        </a>
      }
    </div>
  </div>
}
```

## Loading Skeleton

```html
<!-- แสดงระหว่าง HTMX กำลังโหลด -->
<tr class="htmx-indicator">
  @for (int i = 0; i < 5; i++)
  {
    <tr class="border-b border-gray-100 animate-pulse">
      <td class="px-4 py-3"><div class="h-4 bg-gray-200 rounded w-28"></div></td>
      <td class="px-4 py-3"><div class="h-4 bg-gray-200 rounded w-20"></div></td>
      <td class="px-4 py-3"><div class="h-4 bg-gray-200 rounded w-24"></div></td>
      <td class="px-4 py-3"><div class="h-5 bg-gray-200 rounded-full w-16"></div></td>
      <td class="px-4 py-3"><div class="h-5 bg-gray-200 rounded-full w-20"></div></td>
      <td class="px-4 py-3"><div class="h-4 bg-gray-200 rounded w-16"></div></td>
    </tr>
  }
</tr>
```
