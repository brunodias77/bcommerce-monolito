using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class UserDeletedEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserDeletedEvent(Guid userId)
    {
        UserId = userId;
    }
}
