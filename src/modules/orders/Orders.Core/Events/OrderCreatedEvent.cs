using BuildingBlocks.Domain.Events;
using Orders.Core.Entities;

namespace Orders.Core.Events;

public class OrderCreatedEvent : DomainEvent
{
    public Order Order { get; }
    public override Guid AggregateId => Order.Id;

    public OrderCreatedEvent(Order order)
    {
        Order = order;
    }
}
