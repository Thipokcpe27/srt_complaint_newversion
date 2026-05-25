using SRT.Complaint.Models;

namespace SRT.Complaint.Services;

public interface IApiKeyService
{
    Task<(ApiKey key, string rawKey)> CreateAsync(CreateApiKeyRequest request, CancellationToken ct = default);
    Task<ApiKey?> ValidateAsync(string rawKey, CancellationToken ct = default);
    Task RevokeAsync(int id, int revokedById, string reason, CancellationToken ct = default);
    Task<IReadOnlyList<ApiKey>> ListAsync(CancellationToken ct = default);
}

public record CreateApiKeyRequest(
    string Name,
    string KeyType,
    IReadOnlyList<string> Scopes,
    int RateLimitPerMin,
    string? AllowedIps,
    DateTime? ExpiresAt,
    int CreatedById
);
