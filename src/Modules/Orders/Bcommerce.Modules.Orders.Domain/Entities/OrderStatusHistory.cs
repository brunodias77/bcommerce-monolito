using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Orders.Domain.Enums;

namespace Bcommerce.Modules.Orders.Domain.Entities;

public class OrderStatusHistory : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private OrderStatusHistory() { }

    public OrderStatusHistory(Guid orderId, OrderStatus status, string? reason)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Status = status;
        Reason = reason;
        ChangedAt = DateTime.UtcNow;
    }
}
