using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Processors;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.BackgroundJobs;

[DisallowConcurrentExecution]
public class OutboxProcessorJob(OutboxProcessor processor) : IJob
{
    private readonly OutboxProcessor _processor = processor;

    public async Task Execute(IJobExecutionContext context)
    {
        await _processor.ProcessAsync(context.CancellationToken);
    }
}
