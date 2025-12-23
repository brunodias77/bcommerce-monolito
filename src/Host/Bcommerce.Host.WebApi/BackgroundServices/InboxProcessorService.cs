using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bcommerce.Host.WebApi.BackgroundServices;

public class InboxProcessorService : BackgroundService
{
    private readonly ILogger<InboxProcessorService> _logger;

    public InboxProcessorService(ILogger<InboxProcessorService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inbox Processor Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Trigger inbox processing for modules
            
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
