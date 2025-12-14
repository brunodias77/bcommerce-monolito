using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;

/// <summary>
/// Evento levantado quando um novo usuário é criado.
/// </summary>
[AggregateType("User")]
public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string UserName { get; }

    public override Guid AggregateId => UserId;

    public UserCreatedEvent(Guid userId, string email, string userName)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
    }
}