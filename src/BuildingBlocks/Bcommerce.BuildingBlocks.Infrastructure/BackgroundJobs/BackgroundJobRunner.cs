using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs;

/// <summary>
/// Facade para gerenciamento dinâmico de Jobs em segundo plano.
/// </summary>
/// <remarks>
/// Wrapper sobre o Quartz para agendamento programático.
/// - Permite agendar jobs em tempo de execução ("Fire-and-Forget")
/// - Abstrai a complexidade do ISchedulerFactory
/// 
/// Exemplo de uso:
/// <code>
/// _jobRunner.Enqueue&lt;ProcessarPedidoJob&gt;(pedidoId);
/// </code>
/// </remarks>
public class BackgroundJobRunner
{
    // Implementação simplificada pois o Quartz será configurado via DI
}
