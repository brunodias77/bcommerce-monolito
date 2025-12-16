using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

// Placeholder para lógica de carrinhos abandonados
public class AbandonedCartsJob(ILogger<AbandonedCartsJob> logger) : IJob
{
    private readonly ILogger<AbandonedCartsJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Verificando carrinhos abandonados para envio de e-mails...");
        return Task.CompletedTask;
    }
}
