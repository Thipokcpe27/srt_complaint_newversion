#nullable enable
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRT.Complaint.Services.Adapters;

public class TraffyFonduAdapter : IExternalSystemAdapter
{
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly string _orgId;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<TraffyFonduAdapter> _logger;

    // Token cache — safe because adapter is Singleton
    private string? _cachedToken;
    private long    _tokenExpiry;  // Unix timestamp (seconds)
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TraffyFonduAdapter(
        IConfiguration config,
        IHttpClientFactory httpFactory,
        ILogger<TraffyFonduAdapter> logger)
    {
        _baseUrl    = (config["TraffyFondue:ApiUrl"]   ?? "https://publicapi.traffy.in.th/exchange-api").TrimEnd('/');
        _username   = config["TraffyFondue:Username"]  ?? "";
        _password   = config["TraffyFondue:Password"]  ?? "";
        _orgId      = config["TraffyFondue:OrgId"]     ?? "";
        _httpFactory = httpFactory;
        _logger      = logger;
    }

    public string SystemKey   => "traffy_fondue";
    public string DisplayName => "Traffy Fondue";
    public string Description => "ระบบรับแจ้งปัญหาของ NECTEC/NSTDA";
    public bool   IsConfigured => !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_orgId);

    // ────────────────────────────────────────────────────────────
    //  Fetch
    // ────────────────────────────────────────────────────────────
    public async Task<ExternalFetchResult> FetchNewAsync(CancellationToken ct = default)
    {
        try
        {
            var token  = await GetTokenAsync(ct);
            var client = CreateClient(token);

            var url      = $"{_baseUrl}/get-issues/v1?org_id={_orgId}&duration=week";
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json   = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<TraffyIssuesResponse>(json, _jsonOpts)
                         ?? throw new InvalidOperationException("Response null");

            var items = result.Results.Select(MapToDto).ToList();
            _logger.LogInformation("TraffyFondue fetched {Count} issues", items.Count);
            return new ExternalFetchResult(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TraffyFondue FetchNewAsync failed");
            return new ExternalFetchResult([], ex.Message);
        }
    }

    // ────────────────────────────────────────────────────────────
    //  Push status back to Traffy
    // ────────────────────────────────────────────────────────────
    public async Task PushStatusAsync(string externalId, string newStatus, string? note, CancellationToken ct = default)
    {
        var token  = await GetTokenAsync(ct);
        var client = CreateClient(token);

        var body = JsonSerializer.Serialize(new
        {
            ticket_id = externalId,
            status_id = MapStatusId(newStatus),
            note      = note ?? ""
        });

        var content  = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"{_baseUrl}/update-issue/v1", content, ct);
        response.EnsureSuccessStatusCode();
    }

    // ────────────────────────────────────────────────────────────
    //  JWT token management
    // ────────────────────────────────────────────────────────────
    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (_cachedToken != null && _tokenExpiry > now + 60)
            return _cachedToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (_cachedToken != null && _tokenExpiry > now + 60)
                return _cachedToken;

            var client  = _httpFactory.CreateClient();
            var payload = JsonSerializer.Serialize(new { user = _username, pass = _password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_baseUrl}/get-auth/v1", content, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var auth = JsonSerializer.Deserialize<TraffyAuthResponse>(json, _jsonOpts)
                       ?? throw new InvalidOperationException("Auth response null");

            _cachedToken = auth.Token;
            _tokenExpiry = auth.ExpireTimestamp;
            _logger.LogInformation("TraffyFondue token refreshed, expires {Expiry}", _tokenExpiry);
            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    // ────────────────────────────────────────────────────────────
    //  Mapping helpers
    // ────────────────────────────────────────────────────────────
    private HttpClient CreateClient(string token)
    {
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static ExternalComplaintDto MapToDto(TraffyIssue i) => new(
        ExternalId:    i.TicketId,
        Description:   i.Description ?? "(ไม่มีรายละเอียด)",
        Address:       i.Address,
        CategoryHint:  i.Topic?.FirstOrDefault() ?? i.Type,
        IncidentDate:  ParseTimestamp(i.Timestamp),
        ReporterName:  i.Name,
        ReporterPhone: NormalisePhone(i.Phone),
        RawStatus:     i.Status ?? "รอรับเรื่อง");

    private static DateTime? ParseTimestamp(string? ts)
        => string.IsNullOrEmpty(ts) ? null
           : DateTime.TryParse(ts, out var dt) ? dt : null;

    private static string? NormalisePhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone)) return null;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length is >= 9 and <= 10 ? digits : null;
    }

    private static int MapStatusId(string srtStatus) => srtStatus switch
    {
        "Pending"     => 1,   // รับเรื่อง
        "InProgress"  => 2,   // อยู่ระหว่างดำเนินการ
        "WaitingInfo" => 2,
        "Forwarded"   => 2,
        "UnderReview" => 2,
        "Resolved"    => 3,   // ดำเนินการแล้ว
        "Closed"      => 3,
        "Rejected"    => 4,   // ไม่สามารถดำเนินการ
        _             => 2
    };

    // ────────────────────────────────────────────────────────────
    //  Internal DTOs
    // ────────────────────────────────────────────────────────────
    private sealed class TraffyAuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = "";

        [JsonPropertyName("expire_timestamp")]
        public long ExpireTimestamp { get; set; }
    }

    private sealed class TraffyIssuesResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("results")]
        public List<TraffyIssue> Results { get; set; } = [];
    }

    private sealed class TraffyIssue
    {
        [JsonPropertyName("ticket_id")]
        public string TicketId { get; set; } = "";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("topic")]
        public List<string>? Topic { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("status_id")]
        public int? StatusId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }
    }
}
