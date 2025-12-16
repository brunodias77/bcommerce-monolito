using BuildingBlocks.Domain.Entities;
using Orders.Core.Enums;
using Orders.Core.ValueObjects;

namespace Orders.Core.Entities;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();
    private readonly List<OrderStatusHistory> _statusHistory = new();
    private readonly List<TrackingEvent> _trackingEvents = new();
    private readonly List<Invoice> _invoices = new();
    private readonly List<OrderRefund> _refunds = new();

    public string OrderNumber { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? CartId { get; private set; }
    public Guid? CouponId { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }

    public string? CouponSnapshot { get; private set; } // JSONB
    public OrderStatus Status { get; private set; }

    public AddressSnapshot ShippingAddress { get; private set; }
    public AddressSnapshot? BillingAddress { get; private set; }

    public ShippingMethod ShippingMethod { get; private set; }
    public string? ShippingCarrier { get; private set; }
    public string? TrackingCode { get; private set; }
    public string? TrackingUrl { get; private set; }
    public DateTime? EstimatedDeliveryAt { get; private set; }

    public string PaymentMethod { get; private set; } // enum as string? shared.payment_method_type

    public CancellationReason? CancellationReason { get; private set; }
    public string? CancellationNotes { get; private set; }
    public Guid? CancelledBy { get; private set; }

    public string? CustomerNotes { get; private set; }
    public string? InternalNotes { get; private set; }



    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyCollection<TrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();
    public IReadOnlyCollection<Invoice> Invoices => _invoices.AsReadOnly();
    public IReadOnlyCollection<OrderRefund> Refunds => _refunds.AsReadOnly();

    protected Order() { }

    public Order(
        string orderNumber,
        Guid userId,
        Guid? cartId,
        AddressSnapshot shippingAddress,
        AddressSnapshot? billingAddress,
        ShippingMethod shippingMethod,
        string paymentMethod,
        string? customerNotes
    )
    {
        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        UserId = userId;
        CartId = cartId;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        ShippingMethod = shippingMethod;
        PaymentMethod = paymentMethod;
        CustomerNotes = customerNotes;
        
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddStatusHistory(null, OrderStatus.Pending, "Order Created");
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        CalculateTotals();
    }

    public void CalculateTotals()
    {
        Subtotal = _items.Sum(i => i.Subtotal);
        // Recalculate Total based on modifiers (discount, shipping, tax)
        Total = Subtotal - DiscountAmount + ShippingAmount + TaxAmount;
    }

    public void SetShippingCost(decimal amount)
    {
        ShippingAmount = amount;
        CalculateTotals();
    }
    
    public void ApplyDiscount(decimal amount, string? couponSnapshot, Guid? couponId)
    {
        DiscountAmount = amount;
        CouponSnapshot = couponSnapshot;
        CouponId = couponId;
        CalculateTotals();
    }

    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Pending || Status == OrderStatus.PaymentProcessing)
        {
            UpdateStatus(OrderStatus.Paid, "Payment Confirmed");
            PaidAt = DateTime.UtcNow;
        }
    }

    private void UpdateStatus(OrderStatus newStatus, string? reason = null, Guid? changedBy = null)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddStatusHistory(oldStatus, newStatus, reason, changedBy);
    }

    private void AddStatusHistory(OrderStatus? fromStatus, OrderStatus toStatus, string? reason, Guid? changedBy = null)
    {
        _statusHistory.Add(new OrderStatusHistory(Id, fromStatus, toStatus, reason, changedBy, null));
    }
}
