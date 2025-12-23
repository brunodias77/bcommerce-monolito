using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class SessionCreatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Guid SessionId { get; }

    public SessionCreatedEvent(Guid userId, Guid sessionId)
    {
        UserId = userId;
        SessionId = sessionId;
    }
}
