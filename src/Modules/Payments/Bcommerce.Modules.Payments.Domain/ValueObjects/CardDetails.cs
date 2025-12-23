using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Payments.Domain.ValueObjects;

public class CardDetails : ValueObject
{
    public string HolderName { get; }
    public string Number { get; } // Usually masked or tokenized in domain, raw only in secure contexts
    public string ExpirationMonth { get; }
    public string ExpirationYear { get; }
    public string Cvv { get; } // Never store this persistently! Only for transient use.

    public CardDetails(string holderName, string number, string expirationMonth, string expirationYear, string cvv)
    {
        HolderName = holderName;
        Number = number;
        ExpirationMonth = expirationMonth;
        ExpirationYear = expirationYear;
        Cvv = cvv;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return HolderName;
        yield return Number;
        yield return ExpirationMonth;
        yield return ExpirationYear;
         // CVV excluded from equality to be safe, though usage in VO is transient
    }
}
