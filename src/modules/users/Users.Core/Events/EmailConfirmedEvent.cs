using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;


/// <summary>
/// Evento levantado quando o email do usuário é confirmado.
/// </summary>
[AggregateType("User")]
public class EmailConfirmedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public override Guid AggregateId => UserId;

    public EmailConfirmedEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}