using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Job recorrente para limpeza de reservas de estoque orfãs.
/// </summary>
/// <remarks>
/// Remove reservas temporárias que não foram convertidas em pedidos.
/// - Previne "estoque preso" indefinidamente
/// - Complementar ao ExpiredPaymentsJob
/// 
/// Exemplo de uso:
/// <code>
/// // Configurado para rodar a cada hora
/// q.AddJob&lt;ExpiredReservationsCleanupJob&gt;(...);
/// </code>
/// </remarks>
public class ExpiredReservationsCleanupJob(ILogger<ExpiredReservationsCleanupJob> logger) : IJob
{
    private readonly ILogger<ExpiredReservationsCleanupJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Executando limpeza de reservas de estoque expiradas...");
        
        // Aqui injetaria o serviço de domnínio ou repositório para fazer a limpeza
        
        return Task.CompletedTask;
    }
}
