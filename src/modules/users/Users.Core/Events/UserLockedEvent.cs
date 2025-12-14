using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;
/// <summary>
/// Evento levantado quando o usuário é bloqueado.
/// </summary>
[AggregateType("User")]
public class UserLockedEvent : DomainEvent
{
    public Guid UserId { get; }
    public DateTimeOffset LockoutEnd { get; }

    public override Guid AggregateId => UserId;

    public UserLockedEvent(Guid userId, DateTimeOffset lockoutEnd)
    {
        UserId = userId;
        LockoutEnd = lockoutEnd;
    }
}