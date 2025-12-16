using BuildingBlocks.Domain.Events;

namespace Orders.Core.Events;

public class OrderShippedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public override Guid AggregateId => OrderId;

    public OrderShippedEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
