namespace SRT.Complaint.Services;

public class WebhookRetryService(
    IServiceScopeFactory scopeFactory,
    ILogger<WebhookRetryService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
            try
            {
                using var scope = scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IWebhookService>();
                await svc.RetryPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Webhook retry batch failed");
            }
        }
    }
}
