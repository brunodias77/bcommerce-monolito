using BuildingBlocks.Domain.Entities;

namespace Payments.Core.Entities;

public class Chargeback : Entity
{
    public Guid PaymentId { get; private set; }
    public string? GatewayChargebackId { get; private set; }
    public string? ReasonCode { get; private set; }
    public string? ReasonDescription { get; private set; }
    public decimal Amount { get; private set; }
    
    public bool EvidenceSubmitted { get; private set; }
    public DateTime? EvidenceDueAt { get; private set; }
    
    public string Status { get; private set; } // OPEN, WON, LOST
    public string? Result { get; private set; }
    
    public DateTime OpenedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Chargeback() { }

    public Chargeback(Guid paymentId, decimal amount, string? gatewayChargebackId, DateTime openedAt)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Amount = amount;
        GatewayChargebackId = gatewayChargebackId;
        OpenedAt = openedAt;
        Status = "OPEN";
        CreatedAt = DateTime.UtcNow;
    }
}
