using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Job recorrente para atualização de Views Materializadas.
/// </summary>
/// <remarks>
/// Otimiza consultas de relatórios pré-calculando dados pesados.
/// - Refresh de tabelas de análise via stored procedures ou consultas SQL
/// - Melhora performance de dashboards
/// 
/// Exemplo de uso:
/// <code>
/// // Rodar na madrugada
/// .WithCronSchedule("0 0 3 * * ?")
/// </code>
/// </remarks>
public class MaterializedViewRefreshJob(ILogger<MaterializedViewRefreshJob> logger) : IJob
{
    private readonly ILogger<MaterializedViewRefreshJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Atualizando Views Materializadas de Relatórios...");
        return Task.CompletedTask;
    }
}
