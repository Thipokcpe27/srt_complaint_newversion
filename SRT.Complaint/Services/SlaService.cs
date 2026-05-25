using Microsoft.EntityFrameworkCore;
using SRT.Complaint.Data;

namespace SRT.Complaint.Services;

public class SlaService(AppDbContext db) : ISlaService
{
    public DateTime CalculateDeadline(string priority, DateTime from)
    {
        var config = db.SlaConfigs.FirstOrDefault(s => s.Priority == priority);
        var hours = config?.ResolutionHours ?? 168;
        return from.AddHours(hours);
    }
}

public class SlaBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SlaBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await RunChecksAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "SLA background check failed");
            }
        }
    }

    private static async Task RunChecksAsync(IServiceProvider services, CancellationToken ct)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        // Mark SLA breached
        var breached = await db.Complaints
            .Where(c => !c.SlaBreached && c.SlaDeadline.HasValue && c.SlaDeadline < now
                        && c.Status != "Closed" && c.Status != "Resolved" && c.Status != "Rejected")
            .ToListAsync(ct);
        foreach (var c in breached)
            c.SlaBreached = true;

        if (breached.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
