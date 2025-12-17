using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Orders.Domain.Enums;
using Bcommerce.Modules.Orders.Domain.Events;
using Bcommerce.Modules.Orders.Domain.ValueObjects;

namespace Bcommerce.Modules.Orders.Domain.Entities;

public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = new();
    private readonly List<OrderStatusHistory> _statusHistory = new();
    private readonly List<TrackingEvent> _trackingEvents = new();

    public OrderNumber OrderNumber { get; private set; }
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public OrderTotal Total { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; }
    public ShippingMethod ShippingMethod { get; private set; }
    public TrackingCode? TrackingCode { get; private set; }
    public Invoice? Invoice { get; private set; }
    public OrderRefund? Refund { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public CancellationReason? CancellationReason { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyCollection<TrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    private Order() { }

    public static Order Create(Guid userId, OrderNumber orderNumber, ShippingAddress shippingAddress, ShippingMethod shippingMethod, List<OrderItem> items, decimal shippingFee, decimal discount)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderNumber = orderNumber,
            ShippingAddress = shippingAddress,
            ShippingMethod = shippingMethod,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            order._items.Add(item);
        }

        var itemsTotal = items.Sum(i => i.TotalPrice);
        order.Total = new OrderTotal(itemsTotal, shippingFee, discount);
        
        order.AddStatusHistory(OrderStatus.PendingPayment, "Order Created");
        order.AddDomainEvent(new OrderPlacedEvent(order.Id, userId));

        return order;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.PendingPayment) return;

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
        AddStatusHistory(Status, "Payment Confirmed");
        AddDomainEvent(new OrderPaidEvent(Id));
    }

    public void MarkAsShipped(TrackingCode trackingCode)
    {
        if (Status != OrderStatus.Paid) return;

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        TrackingCode = trackingCode;
        AddStatusHistory(Status, "Order Shipped");
        AddDomainEvent(new OrderShippedEvent(Id, trackingCode.Value));
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped) return;

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        AddStatusHistory(Status, "Order Delivered");
        AddDomainEvent(new OrderDeliveredEvent(Id));
    }

    public void Cancel(CancellationReason reason, string? notes = null)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled) return;

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        AddStatusHistory(Status, $"Order Cancelled: {reason}. {notes}");
        AddDomainEvent(new OrderCancelledEvent(Id, reason, notes));
    }

    public void AssignInvoice(Invoice invoice)
    {
        Invoice = invoice;
    }

    public void ProcessRefund(OrderRefund refund)
    {
        Refund = refund;
    }

    private void AddStatusHistory(OrderStatus status, string? reason)
    {
        _statusHistory.Add(new OrderStatusHistory(Id, status, reason));
    }
}
