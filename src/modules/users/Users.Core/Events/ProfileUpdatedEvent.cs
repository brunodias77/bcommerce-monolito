using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;

/// <summary>
/// Evento levantado quando o perfil do usuário é atualizado.
/// </summary>
[AggregateType("Profile")]
public class ProfileUpdatedEvent : DomainEvent
{
    public Guid ProfileId { get; }
    public Guid UserId { get; }

    public override Guid AggregateId => ProfileId;

    public ProfileUpdatedEvent(Guid profileId, Guid userId)
    {
        ProfileId = profileId;
        UserId = userId;
    }
}