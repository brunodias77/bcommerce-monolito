using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;

/// <summary>
/// Evento levantado quando uma nova sessão é criada.
/// </summary>
[AggregateType("Session")]
public class SessionCreatedEvent : DomainEvent
{
    public Guid SessionId { get; }
    public Guid UserId { get; }
    public string? DeviceType { get; }
    public string? IpAddress { get; }

    public override Guid AggregateId => SessionId;

    public SessionCreatedEvent(Guid sessionId, Guid userId, string? deviceType, string? ipAddress)
    {
        SessionId = sessionId;
        UserId = userId;
        DeviceType = deviceType;
        IpAddress = ipAddress;
    }
}