using BuildingBlocks.Domain.Events;

namespace Users.Contracts.Events;

/// <summary>
/// Evento de integração publicado quando o perfil de um usuário é atualizado.
/// Outros módulos podem invalidar cache ou atualizar dados relacionados.
/// </summary>
public record UserProfileUpdatedIntegrationEvent(
    Guid UserId,
    string? FirstName,
    string? LastName,
    DateTime UpdatedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string SourceModule { get; init; } = "Users";
}
