using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class AddressAddedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Guid AddressId { get; }

    public AddressAddedEvent(Guid userId, Guid addressId)
    {
        UserId = userId;
        AddressId = addressId;
    }
}
