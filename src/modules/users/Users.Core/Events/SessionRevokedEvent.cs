using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;

/// <summary>
/// Evento levantado quando uma sessão é revogada.
/// </summary>
[AggregateType("Session")]
public class SessionRevokedEvent : DomainEvent
{
    public Guid SessionId { get; }
    public Guid UserId { get; }
    public string Reason { get; }

    public override Guid AggregateId => SessionId;

    public SessionRevokedEvent(Guid sessionId, Guid userId, string reason)
    {
        SessionId = sessionId;
        UserId = userId;
        Reason = reason;
    }
}

