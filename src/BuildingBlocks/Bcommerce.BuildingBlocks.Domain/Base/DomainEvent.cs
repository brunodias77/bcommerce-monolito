using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Base;

public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
