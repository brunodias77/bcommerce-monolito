
using Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure.Jobs;

// Note: IBackgroundJob is the interface from BuildingBlocks
[DisallowConcurrentExecution]
public class SampleJob : IBackgroundJob
{
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(ILogger<SampleJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Job de Teste Executado em: {Time}", DateTimeOffset.Now);
        await Task.CompletedTask;
    }
}
