using BuildingBlocks.Domain.Events;

namespace Users.Core.Events;

/// <summary>
/// Evento levantado quando um perfil é criado para um usuário.
/// </summary>
[AggregateType("Profile")]
public class ProfileCreatedEvent : DomainEvent
{
    public Guid ProfileId { get; }
    public Guid UserId { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public override Guid AggregateId => ProfileId;

    public ProfileCreatedEvent(Guid profileId, Guid userId, string firstName, string lastName)
    {
        ProfileId = profileId;
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
    }
}