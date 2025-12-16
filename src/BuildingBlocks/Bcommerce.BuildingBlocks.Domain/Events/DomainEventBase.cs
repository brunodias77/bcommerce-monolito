using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Events;

public abstract class DomainEventBase : IDomainEvent
{
    public Guid EventId { get; protected set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
