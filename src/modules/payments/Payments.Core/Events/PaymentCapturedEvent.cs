using BuildingBlocks.Domain.Events;

namespace Payments.Core.Events;

public class PaymentCapturedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public override Guid AggregateId => PaymentId;

    public PaymentCapturedEvent(Guid paymentId, Guid orderId, decimal amount)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
    }
}
