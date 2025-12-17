using Bcommerce.BuildingBlocks.Domain.Abstractions;
using MediatR;

namespace Bcommerce.BuildingBlocks.Domain.Events;

/// <summary>
/// Responsável por publicar eventos de domínio pendentes.
/// </summary>
/// <remarks>
/// Tipicamente invocado pela infraestrutura após SaveChanges.
/// - Coleta eventos de todos os aggregates modificados
/// - Publica via MediatR para handlers registrados
/// - Limpa eventos após publicação
/// 
/// Exemplo de uso:
/// <code>
/// // No DbContext.SaveChangesAsync():
/// public override async Task&lt;int&gt; SaveChangesAsync(CancellationToken ct)
/// {
///     var result = await base.SaveChangesAsync(ct);
///     var aggregates = ChangeTracker.Entries&lt;IAggregateRoot&gt;()
///         .Select(e => e.Entity).ToList();
///     await _dispatcher.DispatchAndClearEvents(aggregates);
///     return result;
/// }
/// </code>
/// </remarks>
public class DomainEventDispatcher(IMediator mediator)
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Publica todos os eventos pendentes e limpa a coleção.
    /// </summary>
    /// <param name="entitiesWithEvents">Entidades com eventos pendentes.</param>
    public async Task DispatchAndClearEvents(IEnumerable<IEntity> entitiesWithEvents)
    {
        foreach (var entity in entitiesWithEvents)
        {
            if (entity is IAggregateRoot aggregateRoot)
            {
                var events = aggregateRoot.DomainEvents.ToArray();
                aggregateRoot.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    await _mediator.Publish(domainEvent);
                }
            }
        }
    }
}
