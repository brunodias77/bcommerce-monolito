using BuildingBlocks.Domain.Entities;
using Payments.Core.Enums;

namespace Payments.Core.Entities;

public class UserPaymentMethod : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string? GatewayCustomerId { get; private set; }
    public string GatewayPaymentMethodId { get; private set; }
    public string GatewayName { get; private set; }
    
    public PaymentMethodType MethodType { get; private set; }
    
    public CardBrand? CardBrand { get; private set; }
    public string? CardLastFour { get; private set; }
    public string? CardHolderName { get; private set; }
    public string? CardExpirationMonth { get; private set; }
    public string? CardExpirationYear { get; private set; }
    
    public string? WalletType { get; private set; }
    public string? WalletEmail { get; private set; }
    
    public bool IsDefault { get; private set; }
    public bool IsValid { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    protected UserPaymentMethod() { }

    public UserPaymentMethod(Guid userId, string gatewayPaymentMethodId, string gatewayName, PaymentMethodType methodType)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GatewayPaymentMethodId = gatewayPaymentMethodId;
        GatewayName = gatewayName;
        MethodType = methodType;
        IsValid = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetCardDetails(CardBrand brand, string lastFour, string holderName, string expMonth, string expYear)
    {
        CardBrand = brand;
        CardLastFour = lastFour;
        CardHolderName = holderName;
        CardExpirationMonth = expMonth;
        CardExpirationYear = expYear;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }
}
