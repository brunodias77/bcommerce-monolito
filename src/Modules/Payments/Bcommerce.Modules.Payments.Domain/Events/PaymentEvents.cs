using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Payments.Domain.Events;

public class PaymentInitiatedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public decimal Amount { get; }

    public PaymentInitiatedEvent(Guid paymentId, Guid orderId, decimal amount)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
    }
}

public class PaymentAuthorizedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }

    public PaymentAuthorizedEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }
}

public class PaymentCapturedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }

    public PaymentCapturedEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }
}

public class PaymentFailedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public string Reason { get; }

    public PaymentFailedEvent(Guid paymentId, Guid orderId, string reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Reason = reason;
    }
}

public class RefundProcessedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public decimal Amount { get; }

    public RefundProcessedEvent(Guid paymentId, decimal amount)
    {
        PaymentId = paymentId;
        Amount = amount;
    }
}
