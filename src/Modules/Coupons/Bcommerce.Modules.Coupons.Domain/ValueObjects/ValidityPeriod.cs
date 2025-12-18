using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Coupons.Domain.ValueObjects;

public class ValidityPeriod : ValueObject
{
    public DateTime StartsAt { get; }
    public DateTime? EndsAt { get; }

    public ValidityPeriod(DateTime startsAt, DateTime? endsAt)
    {
        if (endsAt.HasValue && endsAt < startsAt)
        {
             throw new ArgumentException("End date cannot be before start date.", nameof(endsAt));
        }

        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public bool IsValid(DateTime date)
    {
        return date >= StartsAt && (!EndsAt.HasValue || date <= EndsAt);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartsAt;
        yield return EndsAt ?? DateTime.MaxValue;
    }
}
