using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Orders.Domain.Enums;

namespace Bcommerce.Modules.Orders.Domain.Events;

public class OrderPlacedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }

    public OrderPlacedEvent(Guid orderId, Guid userId)
    {
        OrderId = orderId;
        UserId = userId;
    }
}

public class OrderPaidEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderPaidEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}

public class OrderShippedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string TrackingCode { get; }

    public OrderShippedEvent(Guid orderId, string trackingCode)
    {
        OrderId = orderId;
        TrackingCode = trackingCode;
    }
}

public class OrderDeliveredEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderDeliveredEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}

public class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public CancellationReason Reason { get; }
    public string? Notes { get; }

    public OrderCancelledEvent(Guid orderId, CancellationReason reason, string? notes)
    {
        OrderId = orderId;
        Reason = reason;
        Notes = notes;
    }
}
