using BuildingBlocks.Domain.Events;

namespace Orders.Core.Events;

public class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string Reason { get; }
    public override Guid AggregateId => OrderId;

    public OrderCancelledEvent(Guid orderId, string reason)
    {
        OrderId = orderId;
        Reason = reason;
    }
}
