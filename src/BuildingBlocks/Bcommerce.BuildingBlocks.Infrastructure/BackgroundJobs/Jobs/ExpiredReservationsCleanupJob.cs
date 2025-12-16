using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

// Placeholder para lógica de limpeza de reservas expiradas de estoque
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
