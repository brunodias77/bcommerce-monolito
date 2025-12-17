using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class AddressUpdatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Guid AddressId { get; }

    public AddressUpdatedEvent(Guid userId, Guid addressId)
    {
        UserId = userId;
        AddressId = addressId;
    }
}
