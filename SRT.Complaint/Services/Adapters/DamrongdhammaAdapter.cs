#nullable enable
namespace SRT.Complaint.Services.Adapters;

public class DamrongdhammaAdapter(IConfiguration config) : IExternalSystemAdapter
{
    private readonly string _apiUrl = config["Damrongdhamma:ApiUrl"] ?? "";
    private readonly string _apiKey = config["Damrongdhamma:ApiKey"] ?? "";

    public string SystemKey   => "damrongdhamma";
    public string DisplayName => "ศูนย์ดำรงธรรม";
    public string Description => "ศูนย์รับเรื่องราวร้องทุกข์กระทรวงมหาดไทย";
    public bool   IsConfigured => !string.IsNullOrEmpty(_apiUrl);

    public Task<ExternalFetchResult> FetchNewAsync(CancellationToken ct = default)
    {
        // TODO: Implement when API spec is received from ศูนย์ดำรงธรรม
        throw new NotImplementedException(
            "ศูนย์ดำรงธรรม adapter ยังไม่ได้ตั้งค่า — รอ API spec และ credentials");
    }

    public Task PushStatusAsync(string externalId, string newStatus, string? note, CancellationToken ct = default)
    {
        // TODO: Implement when API spec is received
        throw new NotImplementedException(
            "ศูนย์ดำรงธรรม push status ยังไม่ได้ implement — รอ API spec");
    }
}
