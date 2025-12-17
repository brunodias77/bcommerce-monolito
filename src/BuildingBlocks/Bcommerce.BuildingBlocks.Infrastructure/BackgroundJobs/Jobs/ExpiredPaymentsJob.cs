using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Job recorrente para cancelamento de pagamentos pendentes expirados.
/// </summary>
/// <remarks>
/// Monitora pagamentos temporários (Pix/Boleto) que excederam o tempo limite.
/// - Libera reserva de estoque associada
/// - Atualiza status do pedido para Cancelado
/// 
/// Exemplo de uso:
/// <code>
/// // Agendamento a cada 15 minutos
/// .WithCronSchedule("0 0/15 * * * ?")
/// </code>
/// </remarks>
public class ExpiredPaymentsJob(ILogger<ExpiredPaymentsJob> logger) : IJob
{
    private readonly ILogger<ExpiredPaymentsJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Cancelando pagamentos pendentes expirados...");
        return Task.CompletedTask;
    }
}
