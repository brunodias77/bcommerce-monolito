using MassTransit;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;

/// <summary>
/// Filtro placeholder para garantia de idempotência.
/// </summary>
/// <typeparam name="T">Tipo da mensagem.</typeparam>
/// <remarks>
/// Estrutura base para implementação futura de idempotência.
/// - Pode verificar duplicidade de mensagens via ID
/// - Prepara o terreno para o padrão Exactly-Once
/// 
/// Exemplo de uso:
/// <code>
/// cfg.UseConsumeFilter(typeof(IdempotencyFilter&lt;&gt;), context);
/// </code>
/// </remarks>
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
