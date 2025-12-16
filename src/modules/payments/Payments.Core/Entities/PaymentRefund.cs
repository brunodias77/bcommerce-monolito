using BuildingBlocks.Domain.Entities;

namespace Payments.Core.Entities;

public class PaymentRefund : Entity
{
    public Guid PaymentId { get; private set; }
    public string IdempotencyKey { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; }
    
    public string? GatewayRefundId { get; private set; }
    public string? GatewayResponse { get; private set; }
    
    public string Status { get; private set; } // PENDING, PROCESSED, FAILED
    public Guid? OrderRefundId { get; private set; }
    
    public DateTime? ProcessedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected PaymentRefund() { }

    public PaymentRefund(Guid paymentId, string idempotencyKey, decimal amount, string reason, Guid? orderRefundId)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        IdempotencyKey = idempotencyKey;
        Amount = amount;
        Reason = reason;
        OrderRefundId = orderRefundId;
        Status = "PENDING";
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed(string? gatewayRefundId)
    {
        Status = "PROCESSED";
        GatewayRefundId = gatewayRefundId;
        ProcessedAt = DateTime.UtcNow;
    }
}
