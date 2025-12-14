using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;

/// <summary>
/// Evento levantado quando um novo endereço é adicionado.
/// </summary>
[AggregateType("Address")]
public class AddressAddedEvent : DomainEvent
{
    public Guid AddressId { get; }
    public Guid UserId { get; }
    public string City { get; }
    public string State { get; }
    public bool IsDefault { get; }

    public override Guid AggregateId => AddressId;

    public AddressAddedEvent(Guid addressId, Guid userId, string city, string state, bool isDefault)
    {
        AddressId = addressId;
        UserId = userId;
        City = city;
        State = state;
        IsDefault = isDefault;
    }
}