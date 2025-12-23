using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bcommerce.Host.WebApi.BackgroundServices;

public class OutboxProcessorService : BackgroundService
{
    private readonly ILogger<OutboxProcessorService> _logger;
    // Inject Processors from BuildingBlocks

    public OutboxProcessorService(ILogger<OutboxProcessorService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            // Trigger outbox processing for modules
            // await _outboxProcessor.ProcessAsync(stoppingToken);
            
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
