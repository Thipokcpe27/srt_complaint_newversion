# Create Razor Page with Service

สร้าง Razor Page พร้อม Service, Interface, และ register DI อัตโนมัติ

## Usage

```
/create-page PageName FolderPath
```

Example: `/create-page ComplaintDetail Staff`

## Steps

1. สร้าง Service Interface ใน `Services/IPageNameService.cs`
2. สร้าง Service Implementation ใน `Services/PageNameService.cs`
3. สร้าง Razor Page ใน `Pages/{FolderPath}/PageName.cshtml`
4. สร้าง PageModel ใน `Pages/{FolderPath}/PageName.cshtml.cs` พร้อม inject Service
5. Register Service ใน `Program.cs`: `builder.Services.AddScoped<IPageNameService, PageNameService>()`
6. ตรวจสอบว่าไม่มี compilation error
7. บอก path ที่สร้างให้ user

## Template Code

### Service Interface
```csharp
namespace SRT.Complaint.Services;

public interface I{PageName}Service
{
    // TODO: Add methods
}
```

### Service Implementation
```csharp
namespace SRT.Complaint.Services;

public class {PageName}Service : I{PageName}Service
{
    private readonly AppDbContext _context;
    
    public {PageName}Service(AppDbContext context)
    {
        _context = context;
    }
    
    // TODO: Implement methods
}
```

### PageModel
```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.{FolderPath};

public class {PageName}Model : PageModel
{
    private readonly I{PageName}Service _service;
    
    public {PageName}Model(I{PageName}Service service)
    {
        _service = service;
    }
    
    public void OnGet()
    {
    }
}
```

### Razor View
```cshtml
@page
@model SRT.Complaint.Pages.{FolderPath}.{PageName}Model
@{
    ViewData["Title"] = "{PageName}";
}

<h2>@ViewData["Title"]</h2>

<!-- TODO: Add content -->
```
