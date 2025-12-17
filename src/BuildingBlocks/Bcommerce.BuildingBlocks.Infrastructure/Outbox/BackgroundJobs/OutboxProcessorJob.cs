using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Processors;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.BackgroundJobs;

/// <summary>
/// Job do Quartz para processamento recorrente do Outbox.
/// </summary>
/// <remarks>
/// Garante que eventos pendentes sejam publicados mesmo em caso de reinício.
/// - Execução única simultânea (DisallowConcurrentExecution)
/// - Aciona o OutboxProcessor periodicamente
/// 
/// Exemplo de uso:
/// <code>
/// // Registro no Quartz
/// q.AddJob&lt;OutboxProcessorJob&gt;(opts => opts.WithIdentity("outbox-job"));
/// </code>
/// </remarks>
[DisallowConcurrentExecution]
public class OutboxProcessorJob(OutboxProcessor processor) : IJob
{
    private readonly OutboxProcessor _processor = processor;

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        await _processor.ProcessAsync(context.CancellationToken);
    }
}
