using Bcommerce.BuildingBlocks.Domain.Abstractions;
using MediatR;

namespace Bcommerce.BuildingBlocks.Domain.Events;

public class DomainEventDispatcher(IMediator mediator)
{
    private readonly IMediator _mediator = mediator;

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
