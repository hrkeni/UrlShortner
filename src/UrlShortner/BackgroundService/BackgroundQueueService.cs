using Microsoft.EntityFrameworkCore;
using UrlShortner.Data;

public class BackgroundQueueService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BackgroundQueueService> _logger;

    public BackgroundQueueService(VisitQueue queue,
        IServiceProvider services,
        ILogger<BackgroundQueueService> logger)
    {
        TaskQueue = queue;
        _services = services;
        _logger = logger;
    }

    public VisitQueue TaskQueue { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"Background Queue Service is running.");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var visit = await TaskQueue.Dequeue(stoppingToken);
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UrlShortnerContext>();
            try
            {
                var shortenedUrl = await context.ShortenedUrls.FirstOrDefaultAsync(s => s.Id == visit.Id, stoppingToken);

                shortenedUrl!.RecordVisit();
                context.Update(shortenedUrl);
                await context.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Visit recorded @{Visit}", visit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred recording @{Visit}.", visit);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}