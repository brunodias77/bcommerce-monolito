using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class AddressDeletedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Guid AddressId { get; }

    public AddressDeletedEvent(Guid userId, Guid addressId)
    {
        UserId = userId;
        AddressId = addressId;
    }
}
