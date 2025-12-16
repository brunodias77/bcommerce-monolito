using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

// Placeholder para inativação de cupons expirados
public class ExpiredCouponsJob(ILogger<ExpiredCouponsJob> logger) : IJob
{
    private readonly ILogger<ExpiredCouponsJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Atualizando status de cupons expirados...");
        return Task.CompletedTask;
    }
}
