using BuildingBlocks.Domain.Events;

namespace Orders.Core.Events;

public class OrderPaidEvent : DomainEvent
{
    public Guid OrderId { get; }
    public override Guid AggregateId => OrderId;

    public OrderPaidEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
