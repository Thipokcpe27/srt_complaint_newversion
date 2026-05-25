# Design Dashboard

สร้าง Dashboard page พร้อม stat cards, charts, และ HTMX real-time update

## Usage

```
/design-dashboard DashboardName [sections...]
```

ตัวอย่าง:
- `/design-dashboard StaffDashboard stats,queue,sla-warning`
- `/design-dashboard AdminDashboard stats,chart,corruption-stats,audit-log`
- `/design-dashboard ApiUsageDashboard stats,request-log,error-rate`

## Steps

1. สร้าง Dashboard layout หลัก (sidebar + content)
2. สร้าง Stat Cards ตาม sections ที่ระบุ
3. เพิ่ม Chart (ใช้ Chart.js CDN)
4. เพิ่ม HTMX auto-refresh ทุก 60 วินาที
5. สร้าง PageModel พร้อม query สถิติ
6. สร้าง Partial Views สำหรับแต่ละ section

## Stat Cards Section

```html
<!-- 4-column stat grid -->
<div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">

  <div class="bg-white rounded-lg border border-gray-200 p-4 shadow-sm"
    hx-get="/staff/stats/today"
    hx-trigger="load, every 60s"
    hx-swap="innerHTML">
    <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">รับวันนี้</p>
    <p class="text-3xl font-semibold text-srt-navy mt-1">@Model.TodayCount</p>
    <p class="text-xs text-gray-400 mt-1">
      <span class="text-green-600">↑ @Model.TodayChangePercent%</span> จากเมื่อวาน
    </p>
  </div>

  <div class="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
    <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">กำลังดำเนินการ</p>
    <p class="text-3xl font-semibold text-blue-600 mt-1">@Model.InProgressCount</p>
    <p class="text-xs text-gray-400 mt-1">@Model.AssignedCount รับแล้ว / @Model.UnassignedCount รอรับ</p>
  </div>

  <div class="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
    <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">ใกล้ครบ SLA</p>
    <p class="text-3xl font-semibold text-amber-600 mt-1">@Model.SlaWarningCount</p>
    <p class="text-xs text-gray-400 mt-1">เหลือน้อยกว่า 20%</p>
  </div>

  <div class="bg-white rounded-lg border border-gray-200 p-4 shadow-sm
    @(Model.SlaBreachedCount > 0 ? "border-red-300 bg-red-50" : "")">
    <p class="text-xs font-medium @(Model.SlaBreachedCount > 0 ? "text-red-500" : "text-gray-500") uppercase tracking-wide">
      เกิน SLA
    </p>
    <p class="text-3xl font-semibold text-red-600 mt-1">@Model.SlaBreachedCount</p>
    @if (Model.SlaBreachedCount > 0)
    {
      <p class="text-xs text-red-500 mt-1 font-medium">⚠ ต้องดำเนินการด่วน</p>
    }
  </div>

</div>
```

## Bar Chart (Chart.js)

```html
<!-- ใน _Layout.cshtml หรือ section scripts -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- HTML -->
<div class="bg-white rounded-lg border border-gray-200 p-5 shadow-sm mb-6">
  <div class="flex items-center justify-between mb-4">
    <h2 class="text-sm font-semibold text-gray-900">จำนวนเรื่องร้องเรียน 7 วันล่าสุด</h2>
    <span class="text-xs text-gray-400">อัปเดตอัตโนมัติทุก 60 วินาที</span>
  </div>
  <canvas id="weeklyChart" height="80"></canvas>
</div>

<!-- Script -->
<script>
const ctx = document.getElementById('weeklyChart');
new Chart(ctx, {
  type: 'bar',
  data: {
    labels: @Html.Raw(Json.Serialize(Model.ChartLabels)),
    datasets: [{
      label: 'จำนวนเรื่อง',
      data: @Html.Raw(Json.Serialize(Model.ChartData)),
      backgroundColor: '#1A4A9A',
      borderRadius: 6,
    }]
  },
  options: {
    responsive: true,
    plugins: {
      legend: { display: false },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { precision: 0 },
        grid: { color: '#F3F4F6' }
      },
      x: {
        grid: { display: false }
      }
    }
  }
});
</script>
```

## SLA Warning List

```html
<div class="bg-white rounded-lg border border-amber-200 p-5 shadow-sm mb-6">
  <h2 class="text-sm font-semibold text-gray-900 mb-3 flex items-center gap-2">
    <i class="ti ti-clock text-amber-500" aria-hidden="true"></i>
    เรื่องที่ใกล้ครบ SLA
  </h2>

  @if (!Model.SlaWarningItems.Any())
  {
    <p class="text-sm text-gray-400 text-center py-4">ไม่มีเรื่องที่ใกล้ครบ SLA</p>
  }
  else
  {
    <div class="space-y-2">
      @foreach (var item in Model.SlaWarningItems)
      {
        <div class="flex items-center gap-3 p-3 rounded-md
          @(item.SlaBreached ? "bg-red-50 border border-red-200" : "bg-amber-50 border border-amber-200")">
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">@item.ReferenceNumber</p>
            <p class="text-xs text-gray-500 truncate">@item.CategoryName · @item.ReporterName</p>
          </div>
          <div class="text-right flex-shrink-0">
            <p class="text-xs font-medium @(item.SlaBreached ? "text-red-600" : "text-amber-600")">
              @(item.SlaBreached ? "เกิน SLA แล้ว" : item.SlaRemainingText)
            </p>
          </div>
          <a href="/staff/case/@item.Id"
            class="text-xs text-srt-navy hover:underline font-medium flex-shrink-0">
            ดู →
          </a>
        </div>
      }
    </div>
  }
</div>
```

## Workload List

```html
<div class="bg-white rounded-lg border border-gray-200 p-5 shadow-sm">
  <h2 class="text-sm font-semibold text-gray-900 mb-3">Workload ทีม</h2>
  <div class="space-y-3">
    @foreach (var officer in Model.TeamWorkload)
    {
      <div class="flex items-center gap-3">
        <div class="w-8 h-8 rounded-full bg-srt-navy/10 flex items-center justify-center
                    text-xs font-medium text-srt-navy flex-shrink-0">
          @officer.Initials
        </div>
        <div class="flex-1 min-w-0">
          <div class="flex items-center justify-between mb-1">
            <p class="text-sm text-gray-700 truncate">@officer.FullName</p>
            <p class="text-xs text-gray-500 ml-2">@officer.OpenCases เรื่อง</p>
          </div>
          <div class="w-full bg-gray-100 rounded-full h-1.5">
            <div class="bg-srt-navy h-1.5 rounded-full transition-all"
              style="width: @(Math.Min(officer.LoadPercent, 100))%"></div>
          </div>
        </div>
      </div>
    }
  </div>
</div>
```
