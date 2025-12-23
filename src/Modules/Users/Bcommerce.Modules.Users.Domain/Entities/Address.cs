using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Users.Domain.ValueObjects;

namespace Bcommerce.Modules.Users.Domain.Entities;

public class Address : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }

    public string? Label { get; private set; } // Casa, Trabalho, etc.
    public string? RecipientName { get; private set; }
    public string Street { get; private set; }
    public string? Number { get; private set; }
    public string? Complement { get; private set; }
    public string? Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public PostalCode PostalCode { get; private set; }
    public string Country { get; private set; } = "BR";

    public GeoLocation? Location { get; private set; }
    public string? IbgeCode { get; private set; }

    public bool IsDefault { get; private set; }
    public bool IsBillingAddress { get; private set; }
    
    // Controle soft delete
    public DateTime? DeletedAt { get; private set; }

    protected Address() { }

    public Address(
        Guid userId,
        string street,
        string city,
        string state,
        PostalCode postalCode,
        string? number,
        string? complement,
        string? neighborhood,
        string? label,
        string? recipientName)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        Label = label;
        RecipientName = recipientName;
        IsDefault = false;
    }

    public void SetLocation(GeoLocation location)
    {
        Location = location;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void RemoveDefault()
    {
        IsDefault = false;
    }

    public void MarkAsDeleted()
    {
        DeletedAt = DateTime.UtcNow;
    }
}
