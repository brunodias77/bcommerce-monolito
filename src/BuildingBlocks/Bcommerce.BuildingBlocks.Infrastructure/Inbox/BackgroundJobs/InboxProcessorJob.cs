using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Processors;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.BackgroundJobs;

[DisallowConcurrentExecution]
public class InboxProcessorJob(InboxProcessor processor) : IJob
{
    private readonly InboxProcessor _processor = processor;

    public async Task Execute(IJobExecutionContext context)
    {
        await _processor.ProcessAsync(context.CancellationToken);
    }
}
