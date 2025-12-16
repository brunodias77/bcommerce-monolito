using BuildingBlocks.Domain.Events;

namespace Payments.Core.Events;

public class PaymentFailedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public string Reason { get; }
    public override Guid AggregateId => PaymentId;

    public PaymentFailedEvent(Guid paymentId, Guid orderId, string reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Reason = reason;
    }
}
