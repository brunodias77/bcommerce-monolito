using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class ProfileUpdatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Guid ProfileId { get; }

    public ProfileUpdatedEvent(Guid userId, Guid profileId)
    {
        UserId = userId;
        ProfileId = profileId;
    }
}
