using BuildingBlocks.Domain.Entities;

namespace Orders.Core.Entities;

public class OrderRefund : Entity
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; }
    public string? RefundMethod { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string? GatewayRefundId { get; private set; }
    public string Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected OrderRefund() { }

    public OrderRefund(Guid orderId, decimal amount, string reason, string? refundMethod, Guid? paymentId, string? gatewayRefundId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        Reason = reason;
        RefundMethod = refundMethod;
        PaymentId = paymentId;
        GatewayRefundId = gatewayRefundId;
        Status = "PENDING";
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        Status = "PROCESSED";
        ProcessedAt = DateTime.UtcNow;
    }
}
