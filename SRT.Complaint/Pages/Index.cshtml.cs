#nullable enable
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages;

public class IndexModel(IContentBlockService contentService) : PageModel
{
    public Dictionary<string, ContentBlock> HomeBlocks { get; private set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "หน้าแรก";
        HomeBlocks = await contentService.GetHomeBlocksAsync();
    }
}
