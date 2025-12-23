using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Payments.Domain.Entities;

public class PaymentRefund : Entity<Guid>
{
    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; }
    public DateTime RefundedAt { get; private set; }

    private PaymentRefund() { }

    public PaymentRefund(Guid paymentId, decimal amount, string reason)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Amount = amount;
        Reason = reason;
        RefundedAt = DateTime.UtcNow;
    }
}
