using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Cart.Domain.ValueObjects;

public class SessionId : ValueObject
{
    public Guid Value { get; }

    public SessionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SessionId cannot be empty", nameof(value));
        }

        Value = value;
    }
    
    public static implicit operator Guid(SessionId sessionId) => sessionId.Value;
    public static implicit operator SessionId(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
