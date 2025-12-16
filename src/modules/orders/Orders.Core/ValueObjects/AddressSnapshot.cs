using BuildingBlocks.Domain.Models;

namespace Orders.Core.ValueObjects;

public class AddressSnapshot : ValueObject
{
    public string RecipientName { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }

    public AddressSnapshot(
        string recipientName,
        string street,
        string number,
        string complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country)
    {
        RecipientName = recipientName;
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
        yield return RecipientName;
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
