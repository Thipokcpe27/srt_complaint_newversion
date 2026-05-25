using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;
using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public class ApiKeyService(AppDbContext db, IAuditService auditService) : IApiKeyService
{
    public async Task<(ApiKey key, string rawKey)> CreateAsync(CreateApiKeyRequest request, CancellationToken ct = default)
    {
        var rawKey = $"srt_{(request.KeyType == "Internal" ? "int" : "live")}_{GenerateRandomKey(32)}";
        var prefix = rawKey[..Math.Min(8, rawKey.Length)];
        var hash = HashKey(rawKey);

        var key = new ApiKey
        {
            Name = request.Name,
            KeyType = request.KeyType,
            KeyPrefix = prefix,
            KeyHash = hash,
            RateLimitPerMin = request.RateLimitPerMin,
            AllowedIps = request.AllowedIps,
            ExpiresAt = request.ExpiresAt,
            CreatedById = request.CreatedById,
            CreatedAt = DateTime.UtcNow,
            Scopes = request.Scopes.Select(s => new ApiKeyScope { Scope = s }).ToList()
        };

        db.ApiKeys.Add(key);
        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("CreateApiKey", request.CreatedById, null, "ApiKey", key.Id.ToString(), new { request.Name, request.KeyType }, null, ct);

        return (key, rawKey);
    }

    public async Task<ApiKey?> ValidateAsync(string rawKey, CancellationToken ct = default)
    {
        var hash = HashKey(rawKey);
        return await db.ApiKeys
            .Include(k => k.Scopes)
            .FirstOrDefaultAsync(k => k.KeyHash == hash && k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow), ct);
    }

    public async Task RevokeAsync(int id, int revokedById, string reason, CancellationToken ct = default)
    {
        var key = await db.ApiKeys.FindAsync([id], ct)
            ?? throw new InvalidOperationException("API key not found");
        key.IsActive = false;
        key.RevokedAt = DateTime.UtcNow;
        key.RevokedById = revokedById;
        key.RevokedReason = reason;
        await db.SaveChangesAsync(ct);
        await auditService.LogAsync("RevokeApiKey", revokedById, null, "ApiKey", id.ToString(), new { reason }, null, ct);
    }

    public async Task<IReadOnlyList<ApiKey>> ListAsync(CancellationToken ct = default)
        => await db.ApiKeys.Include(k => k.Scopes).OrderByDescending(k => k.CreatedAt).ToListAsync(ct);

    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerateRandomKey(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")[..length];
    }
}
