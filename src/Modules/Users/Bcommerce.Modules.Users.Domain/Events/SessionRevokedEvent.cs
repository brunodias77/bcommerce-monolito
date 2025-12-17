using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class SessionRevokedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Guid SessionId { get; }
    public string Reason { get; }

    public SessionRevokedEvent(Guid userId, Guid sessionId, string reason)
    {
        UserId = userId;
        SessionId = sessionId;
        Reason = reason;
    }
}
