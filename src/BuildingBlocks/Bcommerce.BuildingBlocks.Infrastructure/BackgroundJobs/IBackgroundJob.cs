namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs;

/// <summary>
/// Abstração para jobs de processamento em segundo plano.
/// </summary>
/// <remarks>
/// Contrato para tarefas que rodam fora do ciclo de vida da requisição HTTP.
/// - Desacopla a implementação do mecanismo de agendamento (Quartz/Hangfire)
/// - Facilita testes unitários de jobs
/// 
/// Exemplo de uso:
/// <code>
/// public class CleanupJob : IBackgroundJob { ... }
/// </code>
/// </remarks>
public interface IBackgroundJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
