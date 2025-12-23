using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Payments.Domain.Entities;

public class Chargeback : Entity<Guid>
{
    public Guid PaymentId { get; private set; }
    public string ReasonCode { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private Chargeback() { }

    public Chargeback(Guid paymentId, string reasonCode, decimal amount)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        ReasonCode = reasonCode;
        Amount = amount;
        OccurredAt = DateTime.UtcNow;
    }
}
