# Create API Endpoint

สร้าง API Controller พร้อม Authorization, Scope check, และ Logging

## Usage

```
/create-api EndpointName HttpMethod RequiredScope
```

Example: `/create-api GetComplaintStatus GET complaints:status`

## Steps

1. สร้าง Controller ใน `Controllers/Api/{EndpointName}Controller.cs`
2. เพิ่ม `[Authorize]` และ `[RequireScope("{RequiredScope}")]` attribute
3. เพิ่ม method ตาม HttpMethod
4. เพิ่ม logging ใน method
5. เพิ่ม ApiRequestLog ใน finally block
6. สร้าง DTO model สำหรับ Request/Response (ถ้าจำเป็น)
7. ตรวจสอบว่าไม่มี compilation error
8. แสดง curl command ตัวอย่างสำหรับทดสอบ

## Template Code

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRT.Complaint.Services;

namespace SRT.Complaint.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class {EndpointName}Controller : ControllerBase
{
    private readonly ILogger<{EndpointName}Controller> _logger;
    private readonly IApiRequestLogService _logService;
    
    public {EndpointName}Controller(
        ILogger<{EndpointName}Controller> logger,
        IApiRequestLogService logService)
    {
        _logger = logger;
        _logService = logService;
    }
    
    [Http{HttpMethod}]
    [RequireScope("{RequiredScope}")]
    public async Task<IActionResult> {MethodName}()
    {
        try
        {
            _logger.LogInformation("{EndpointName} called");
            
            // TODO: Implement logic
            
            return Ok(new { message = "Success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {EndpointName}");
            return StatusCode(500, new { error = "Internal server error" });
        }
        finally
        {
            await _logService.LogRequestAsync(
                apiKeyId: GetApiKeyIdFromToken(),
                httpMethod: "{HttpMethod}",
                endpoint: Request.Path,
                responseStatus: Response.StatusCode
            );
        }
    }
    
    private int GetApiKeyIdFromToken()
    {
        // Extract from JWT claims
        return int.Parse(User.FindFirst("ApiKeyId")?.Value ?? "0");
    }
}
```

## Test Command

```bash
curl -X {HttpMethod} https://www.railway.co.th/complaint/api/{endpoint} \
  -H "X-API-Key: srt_test_..." \
  -H "Content-Type: application/json"
```
