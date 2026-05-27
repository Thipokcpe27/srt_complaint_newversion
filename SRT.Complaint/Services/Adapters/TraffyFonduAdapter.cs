#nullable enable
namespace SRT.Complaint.Services.Adapters;

public class TraffyFonduAdapter(IConfiguration config) : IExternalSystemAdapter
{
    private readonly string _apiUrl    = config["TraffyFondue:ApiUrl"]    ?? "";
    private readonly string _apiKey    = config["TraffyFondue:ApiKey"]    ?? "";
    private readonly string _keyword   = config["TraffyFondue:FilterKeyword"] ?? "รถไฟ";

    public string SystemKey   => "traffy_fondue";
    public string DisplayName => "Traffy Fondue";
    public string Description => "ระบบรับแจ้งปัญหาของ NECTEC/NSTDA";
    public bool   IsConfigured => !string.IsNullOrEmpty(_apiUrl);

    public Task<ExternalFetchResult> FetchNewAsync(CancellationToken ct = default)
    {
        // TODO: Implement when API spec is received
        // Expected: GET {ApiUrl}?keyword={_keyword}&apikey={_apiKey}
        // Response: GeoJSON FeatureCollection
        throw new NotImplementedException(
            "Traffy Fondue adapter ยังไม่ได้ตั้งค่า — รอ API spec และ key จาก NECTEC");
    }

    public Task PushStatusAsync(string externalId, string newStatus, string? note, CancellationToken ct = default)
    {
        // TODO: Implement when API spec is received
        // Expected: PATCH {ApiUrl}/tickets/{externalId}
        // Body: { state: MapStatus(newStatus), comment: note }
        throw new NotImplementedException(
            "Traffy Fondue push status ยังไม่ได้ implement — รอ API spec");
    }

    // Status mapping (เติมเมื่อได้ spec)
    // private static string MapStatus(string srtStatus) => srtStatus switch
    // {
    //     "InProgress" => "inprogress",
    //     "Resolved"   => "resolved",
    //     "Closed"     => "closed",
    //     "Rejected"   => "invalid",
    //     _            => "inprogress"
    // };
}
