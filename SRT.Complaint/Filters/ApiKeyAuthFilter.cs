#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using SRT.Complaint.Services;

namespace SRT.Complaint.Filters;

public class ApiKeyAuthFilter(IApiKeyService apiKeyService, IMemoryCache cache) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var rawKey) ||
            string.IsNullOrWhiteSpace(rawKey))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "X-API-Key header required" });
            return;
        }

        var apiKey = await apiKeyService.ValidateAsync(rawKey!);
        if (apiKey == null)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid or expired API key" });
            return;
        }

        // IP whitelist
        if (!string.IsNullOrWhiteSpace(apiKey.AllowedIps))
        {
            var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var allowed  = apiKey.AllowedIps
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!allowed.Any(ip => MatchesIp(clientIp, ip)))
            {
                context.Result = new ObjectResult(new { error = "IP address not in whitelist" }) { StatusCode = 403 };
                return;
            }
        }

        // Rate limit — fixed window per minute
        var cacheKey = $"ratelimit:{apiKey.Id}:{DateTime.UtcNow:yyyyMMddHHmm}";
        var count = cache.GetOrCreate(cacheKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return 0;
        });
        if (count >= apiKey.RateLimitPerMin)
        {
            context.Result = new ObjectResult(new { error = "Rate limit exceeded", retryAfter = "60s" }) { StatusCode = 429 };
            return;
        }
        cache.Set(cacheKey, count + 1, TimeSpan.FromMinutes(2));

        context.HttpContext.Items["ApiKey"] = apiKey;
        await next();
    }

    private static bool MatchesIp(string clientIp, string allowedEntry)
    {
        // Exact match
        if (clientIp == allowedEntry) return true;
        // CIDR prefix match (simplified: compare network portion before '/')
        if (allowedEntry.Contains('/'))
        {
            var networkPart = allowedEntry.Split('/')[0].TrimEnd('.');
            return clientIp.StartsWith(networkPart);
        }
        return false;
    }
}
