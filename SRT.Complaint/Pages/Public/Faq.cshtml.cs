using Microsoft.AspNetCore.Mvc.RazorPages;
using SRT.Complaint.Models;
using SRT.Complaint.Services;

namespace SRT.Complaint.Pages.Public;

public class FaqModel(IFaqService faqService) : PageModel
{
    public List<IGrouping<string, FaqItem>> Groups { get; set; } = [];

    public async Task OnGetAsync()
    {
        var items = await faqService.GetAllAsync(activeOnly: true);
        var order = new[] { "ทั่วไป", "การยื่นเรื่องร้องเรียน", "การติดตามสถานะ", "การแจ้งเบาะแสทุจริต" };
        Groups = items
            .GroupBy(x => x.Category ?? "ทั่วไป")
            .OrderBy(g => {
                var i = Array.IndexOf(order, g.Key);
                return i < 0 ? 99 : i;
            })
            .ToList();
    }
}
