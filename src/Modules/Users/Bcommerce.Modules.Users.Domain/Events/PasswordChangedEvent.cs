using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class PasswordChangedEvent : DomainEvent
{
    public Guid UserId { get; }

    public PasswordChangedEvent(Guid userId)
    {
        UserId = userId;
    }
}
