
using Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Quartz; // Annotations usually fine, but interface is what matters

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class SampleJob : IBackgroundJob
{
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(ILogger<SampleJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job de Teste Executado em: {Time}", DateTimeOffset.Now);
        await Task.CompletedTask;
    }
}
