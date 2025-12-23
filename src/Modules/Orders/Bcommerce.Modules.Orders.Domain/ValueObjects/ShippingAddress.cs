using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Orders.Domain.ValueObjects;

// Leveraging or replicating structure similar to common Address value objects
public class ShippingAddress : ValueObject
{
    public string Street { get; }
    public string Number { get; }
    public string Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public ShippingAddress(string street, string number, string complement, string neighborhood, string city, string state, string zipCode, string country)
    {
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return Complement;
        yield return Neighborhood;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}
