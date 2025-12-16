using BuildingBlocks.Domain.Events;

namespace Orders.Core.Events;

public class OrderDeliveredEvent : DomainEvent
{
    public Guid OrderId { get; }
    public override Guid AggregateId => OrderId;

    public OrderDeliveredEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
