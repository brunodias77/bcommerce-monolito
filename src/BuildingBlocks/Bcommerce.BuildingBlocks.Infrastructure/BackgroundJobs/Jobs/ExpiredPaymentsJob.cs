using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

// Placeholder para expiração de pagamentos (Pix, Boleto)
public class ExpiredPaymentsJob(ILogger<ExpiredPaymentsJob> logger) : IJob
{
    private readonly ILogger<ExpiredPaymentsJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Cancelando pagamentos pendentes expirados...");
        return Task.CompletedTask;
    }
}
