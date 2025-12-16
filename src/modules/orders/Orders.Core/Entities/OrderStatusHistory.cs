using BuildingBlocks.Domain.Entities;
using Orders.Core.Enums;

namespace Orders.Core.Entities;

public class OrderStatusHistory : Entity
{
    public Guid OrderId { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public string? Metadata { get; private set; } // JSONB
    public DateTime CreatedAt { get; private set; }

    protected OrderStatusHistory() { }

    public OrderStatusHistory(Guid orderId, OrderStatus? fromStatus, OrderStatus toStatus, string? reason, Guid? changedBy, string? metadata)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Reason = reason;
        ChangedBy = changedBy;
        Metadata = metadata;
        CreatedAt = DateTime.UtcNow;
    }
}
