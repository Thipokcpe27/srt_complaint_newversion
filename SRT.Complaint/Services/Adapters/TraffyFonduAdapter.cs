#nullable enable
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using SRT.Complaint.Services;

namespace SRT.Complaint.Services.Adapters;

public class TraffyFonduAdapter : IExternalSystemAdapter
{
    private readonly IServiceScopeFactory        _scopeFactory;
    private readonly IHttpClientFactory          _httpFactory;
    private readonly IMemoryCache                _cache;
    private readonly IConfiguration             _config;
    private readonly ILogger<TraffyFonduAdapter> _logger;

    // #10 fix: รวม 3 fields เป็น single volatile record — อ่านเป็น atomic reference เดียว
    // ป้องกัน torn read จากการอ่านนอก lock บน multi-core
    private volatile TokenState? _tokenState;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOpts = new()
        { PropertyNameCaseInsensitive = true };

    public TraffyFonduAdapter(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpFactory,
        IMemoryCache cache,
        IConfiguration config,
        ILogger<TraffyFonduAdapter> logger)
    {
        _scopeFactory = scopeFactory;
        _httpFactory  = httpFactory;
        _cache        = cache;
        _config       = config;
        _logger       = logger;
    }

    public string SystemKey   => "traffy_fondue";
    public string DisplayName => "Traffy Fondue";
    public string Description => "ระบบรับแจ้งปัญหาของ NECTEC/NSTDA";

    // #8 fix: IsConfigured ไม่ block DB — อ่านแค่ cache ถ้ามี ไม่งั้น fallback ไป appsettings
    public bool IsConfigured
    {
        get
        {
            if (_cache.TryGetValue("traffy:settings", out TraffySettings? s) && s is not null)
                return !string.IsNullOrEmpty(s.Username) && !string.IsNullOrEmpty(s.OrgId);

            // cache cold → ใช้ appsettings fallback (ไม่ block DB)
            var username = _config["TraffyFondue:Username"];
            var orgId    = _config["TraffyFondue:OrgId"];
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(orgId);
        }
    }

    // ────────────────────────────────────────────────────────────
    //  Settings — #8 fix: async ตลอด ไม่มี .GetAwaiter().GetResult()
    // ────────────────────────────────────────────────────────────
    private async Task<TraffySettings> LoadSettingsAsync(CancellationToken ct = default)
    {
        const string key = "traffy:settings";
        if (_cache.TryGetValue(key, out TraffySettings? s) && s is not null)
            return s;

        using var scope = _scopeFactory.CreateScope();
        var sysSettings = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();
        var dict = await sysSettings.GetGroupAsync("traffy");

        static string DbOrCfg(Dictionary<string, string> d, string dbKey, string? cfgVal)
        {
            var v = d.GetValueOrDefault(dbKey, "");
            return string.IsNullOrEmpty(v) ? cfgVal ?? "" : v;
        }

        s = new TraffySettings(
            Enabled:       dict.GetValueOrDefault("traffy.enabled", "false") == "true",
            ApiUrl:        DbOrCfg(dict, "traffy.api_url",        _config["TraffyFondue:ApiUrl"] ?? "https://publicapi.traffy.in.th/exchange-api").TrimEnd('/'),
            Username:      DbOrCfg(dict, "traffy.username",       _config["TraffyFondue:Username"]),
            Password:      DbOrCfg(dict, "traffy.password",       _config["TraffyFondue:Password"]),
            OrgId:         DbOrCfg(dict, "traffy.org_id",         _config["TraffyFondue:OrgId"]),
            WebhookSecret: DbOrCfg(dict, "traffy.webhook_secret", _config["TraffyFondue:WebhookSecret"])
        );

        _cache.Set(key, s, TimeSpan.FromSeconds(60));
        return s;
    }

    public static void InvalidateCache(IMemoryCache cache)
    {
        cache.Remove("traffy:settings");
    }

    // ────────────────────────────────────────────────────────────
    //  Fetch
    // ────────────────────────────────────────────────────────────
    public async Task<ExternalFetchResult> FetchNewAsync(CancellationToken ct = default)
    {
        var s = await LoadSettingsAsync(ct);
        if (!s.Enabled)
            return new ExternalFetchResult([], "Traffy Fondue integration is disabled");

        try
        {
            var token  = await GetTokenAsync(s, ct);
            var client = CreateClient(token);
            var url    = $"{s.ApiUrl}/get-issues/v1?org_id={s.OrgId}&duration=week";
            var res    = await client.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();

            var json   = await res.Content.ReadAsStringAsync(ct);
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
    //  Push status — #7 fix: ตรวจ Enabled ก่อนทำงาน
    // ────────────────────────────────────────────────────────────
    public async Task PushStatusAsync(string externalId, string newStatus, string? note, CancellationToken ct = default)
    {
        var s = await LoadSettingsAsync(ct);

        // #7 fix: เมื่อ integration ถูกปิด ไม่ push ไป Traffy
        if (!s.Enabled) return;

        var token   = await GetTokenAsync(s, ct);
        var client  = CreateClient(token);
        var body    = JsonSerializer.Serialize(new { ticket_id = externalId, status_id = MapStatusId(newStatus), note = note ?? "" });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var res     = await client.PatchAsync($"{s.ApiUrl}/update-issue/v1", content, ct);
        res.EnsureSuccessStatusCode();
    }

    // ────────────────────────────────────────────────────────────
    //  JWT token — #10 fix: volatile record ป้องกัน torn read
    // ────────────────────────────────────────────────────────────
    public async Task<string> GetTokenAsync(CancellationToken ct = default)
        => await GetTokenAsync(await LoadSettingsAsync(ct), ct);

    private async Task<string> GetTokenAsync(TraffySettings s, CancellationToken ct)
    {
        var now  = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var hash = $"{s.ApiUrl}|{s.Username}|{s.Password}";

        // #10 fix: อ่าน volatile reference เดียว — atomic บน .NET ทุก platform
        var state = _tokenState;
        if (state is not null && state.Expiry > now + 60 && state.CredHash == hash)
            return state.Token;

        await _tokenLock.WaitAsync(ct);
        try
        {
            now   = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            state = _tokenState;
            if (state is not null && state.Expiry > now + 60 && state.CredHash == hash)
                return state.Token;

            var client  = _httpFactory.CreateClient();
            var payload = JsonSerializer.Serialize(new { user = s.Username, pass = s.Password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res     = await client.PostAsync($"{s.ApiUrl}/get-auth/v1", content, ct);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync(ct);
            var auth = JsonSerializer.Deserialize<TraffyAuthResponse>(json, _jsonOpts)
                       ?? throw new InvalidOperationException("Auth response null");

            // #10 fix: publish record ใหม่ทั้งก้อน — thread อื่นจะเห็น state ที่สอดคล้องกันเสมอ
            _tokenState = new TokenState(auth.Token, auth.ExpireTimestamp, hash);
            _logger.LogInformation("TraffyFondue token refreshed, expires {Expiry}", auth.ExpireTimestamp);
            return auth.Token;
        }
        finally { _tokenLock.Release(); }
    }

    // ────────────────────────────────────────────────────────────
    //  Helpers
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
        => string.IsNullOrEmpty(ts) ? null : DateTime.TryParse(ts, out var dt) ? dt : null;

    private static string? NormalisePhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone)) return null;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length is >= 9 and <= 10 ? digits : null;
    }

    private static int MapStatusId(string srtStatus) => srtStatus switch
    {
        "Pending"     => 1,
        "InProgress"  => 2,
        "WaitingInfo" => 2,
        "Forwarded"   => 2,
        "UnderReview" => 2,
        "Resolved"    => 3,
        "Closed"      => 3,
        "Rejected"    => 4,
        _             => 2
    };

    // ── Internal types ──
    // #10 fix: immutable record — เมื่อ publish แล้ว content ไม่เปลี่ยน, thread-safe
    private sealed record TokenState(string Token, long Expiry, string CredHash);

    private sealed record TraffySettings(
        bool   Enabled,
        string ApiUrl,
        string Username,
        string Password,
        string OrgId,
        string WebhookSecret);

    private sealed class TraffyAuthResponse
    {
        [JsonPropertyName("token")]             public string Token          { get; set; } = "";
        [JsonPropertyName("expire_timestamp")]  public long   ExpireTimestamp { get; set; }
    }

    private sealed class TraffyIssuesResponse
    {
        [JsonPropertyName("count")]   public int              Count   { get; set; }
        [JsonPropertyName("results")] public List<TraffyIssue> Results { get; set; } = [];
    }

    private sealed class TraffyIssue
    {
        [JsonPropertyName("ticket_id")]   public string  TicketId    { get; set; } = "";
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("address")]     public string? Address     { get; set; }
        [JsonPropertyName("type")]        public string? Type        { get; set; }
        [JsonPropertyName("topic")]       public List<string>? Topic { get; set; }
        [JsonPropertyName("timestamp")]   public string? Timestamp   { get; set; }
        [JsonPropertyName("status")]      public string? Status      { get; set; }
        [JsonPropertyName("status_id")]   public int?    StatusId    { get; set; }
        [JsonPropertyName("name")]        public string? Name        { get; set; }
        [JsonPropertyName("phone")]       public string? Phone       { get; set; }
    }
}
