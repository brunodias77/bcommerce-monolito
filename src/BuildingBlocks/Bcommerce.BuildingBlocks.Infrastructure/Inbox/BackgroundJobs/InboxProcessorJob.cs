using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Processors;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.BackgroundJobs;

/// <summary>
/// Job do Quartz para execução recorrente do processamento de Inbox.
/// </summary>
/// <remarks>
/// Agendado para rodar periodicamente e invocar o InboxProcessor.
/// - Impede execução concorrente do mesmo job (DisallowConcurrentExecution)
/// - Garante que mensagens acumuladas sejam processadas em background
/// 
/// Exemplo de uso:
/// <code>
/// // Configurado no Startup:
/// q.AddJob&lt;InboxProcessorJob&gt;(opts => opts.WithIdentity(jobKey));
/// </code>
/// </remarks>
[DisallowConcurrentExecution]
public class InboxProcessorJob(InboxProcessor processor) : IJob
{
    private readonly InboxProcessor _processor = processor;

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        await _processor.ProcessAsync(context.CancellationToken);
    }
}
