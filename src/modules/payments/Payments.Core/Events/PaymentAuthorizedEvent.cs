using BuildingBlocks.Domain.Events;

namespace Payments.Core.Events;

public class PaymentAuthorizedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public override Guid AggregateId => PaymentId;

    public PaymentAuthorizedEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }
}
