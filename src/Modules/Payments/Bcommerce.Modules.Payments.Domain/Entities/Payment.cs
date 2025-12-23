using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Payments.Domain.Enums;
using Bcommerce.Modules.Payments.Domain.Events;
using Bcommerce.Modules.Payments.Domain.ValueObjects;

namespace Bcommerce.Modules.Payments.Domain.Entities;

public class Payment : AggregateRoot<Guid>
{
    private readonly List<PaymentTransaction> _transactions = new();
    
    public Guid OrderId { get; private set; }
    public Guid? CustomerId { get; private set; } // Optional if guest checkout
    public PaymentStatus Status { get; private set; }
    public PaymentMethodType MethodType { get; private set; }
    public PaymentAmount Amount { get; private set; }
    
    // Details (nullable depending on method)
    public PixData? PixData { get; private set; }
    public BoletoData? BoletoData { get; private set; }
    
    // Could link to a saved PaymentMethod
    public Guid? PaymentMethodId { get; private set; }

    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();

    private Payment() { }

    public static Payment Create(Guid orderId, Guid? customerId, PaymentAmount amount, PaymentMethodType methodType)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            MethodType = methodType,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        payment.AddDomainEvent(new PaymentInitiatedEvent(payment.Id, orderId, amount.Value));
        return payment;
    }

    public void MarkAsAuthorized()
    {
        if (Status != PaymentStatus.Pending) return;
        
        Status = PaymentStatus.Authorized;
        AddDomainEvent(new PaymentAuthorizedEvent(Id, OrderId));
    }

    public void MarkAsCaptured()
    {
        // Can capture from Authorized or directly from Pending (e.g. instant pix)
        if (Status == PaymentStatus.Captured || Status == PaymentStatus.Failed) return;
        
        Status = PaymentStatus.Captured;
        AddDomainEvent(new PaymentCapturedEvent(Id, OrderId));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status == PaymentStatus.Captured) return;
        
        Status = PaymentStatus.Failed;
        AddDomainEvent(new PaymentFailedEvent(Id, OrderId, reason));
    }

    public void AddTransaction(PaymentTransaction transaction)
    {
        _transactions.Add(transaction);
        // Could also derive status from transaction result here
    }
    
    public void SetPixData(PixData pixData)
    {
        PixData = pixData;
    }
    
    public void SetBoletoData(BoletoData boletoData)
    {
        BoletoData = boletoData;
    }
}
