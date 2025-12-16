using MassTransit;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;

public class IdempotencyFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        // Placeholder para lógica de idempotência se distinta do Inbox
        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("idempotency");
    }
}
