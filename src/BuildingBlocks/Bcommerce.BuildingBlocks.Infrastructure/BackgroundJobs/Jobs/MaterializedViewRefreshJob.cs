using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

// Placeholder para refresh de materialized views (Relatórios)
public class MaterializedViewRefreshJob(ILogger<MaterializedViewRefreshJob> logger) : IJob
{
    private readonly ILogger<MaterializedViewRefreshJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Atualizando Views Materializadas de Relatórios...");
        return Task.CompletedTask;
    }
}
