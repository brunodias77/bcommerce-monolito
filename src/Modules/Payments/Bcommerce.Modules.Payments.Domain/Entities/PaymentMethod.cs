using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Payments.Domain.Enums;
using Bcommerce.Modules.Payments.Domain.ValueObjects;

namespace Bcommerce.Modules.Payments.Domain.Entities;

public class PaymentMethod : AggregateRoot<Guid>
{
    public string Alias { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public bool IsActive { get; private set; }
    
    // Storing masked card info or tokens if necessary
    public string? LastFourDigits { get; private set; }
    public string? Token { get; private set; }
    public CardBrand? CardBrand { get; private set; }

    private PaymentMethod() { }

    public PaymentMethod(string alias, PaymentMethodType type, string? lastFourDigits, string? token, CardBrand? cardBrand)
    {
        Id = Guid.NewGuid();
        Alias = alias;
        Type = type;
        LastFourDigits = lastFourDigits;
        Token = token;
        CardBrand = cardBrand;
        IsActive = true;
    }
}
