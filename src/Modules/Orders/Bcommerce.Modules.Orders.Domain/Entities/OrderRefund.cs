using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Orders.Domain.Enums;

namespace Bcommerce.Modules.Orders.Domain.Entities;

public class OrderRefund : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; }
    public DateTime RefundedAt { get; private set; }

    private OrderRefund() { }

    public OrderRefund(Guid orderId, decimal amount, string reason)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        Reason = reason;
        RefundedAt = DateTime.UtcNow;
    }
}
