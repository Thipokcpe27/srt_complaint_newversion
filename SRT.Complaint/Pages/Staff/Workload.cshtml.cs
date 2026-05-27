using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Staff;

[Authorize(Policy = "StaffOnly")]
public class WorkloadModel(IComplaintService complaintService) : PageModel
{
    public IReadOnlyList<WorkloadItem> TeamWorkload { get; private set; } = [];
    public int TotalOpenCases { get; private set; }
    public int TotalStaff { get; private set; }
    public int AvgCasesPerStaff { get; private set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Workload ทีม";
        TeamWorkload = await complaintService.GetWorkloadAsync();
        TotalStaff = TeamWorkload.Count;
        TotalOpenCases = TeamWorkload.Sum(w => w.OpenCases);
        AvgCasesPerStaff = TotalStaff > 0
            ? (int)Math.Round((double)TotalOpenCases / TotalStaff)
            : 0;
    }
}
