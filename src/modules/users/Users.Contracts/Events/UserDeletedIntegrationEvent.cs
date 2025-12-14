using BuildingBlocks.Domain.Events;

namespace Users.Contracts.Events;

/// <summary>
/// Evento de integração publicado quando um usuário é deletado (soft delete).
/// Outros módulos devem limpar dados relacionados ao usuário.
/// </summary>
public record UserDeletedIntegrationEvent(
    Guid UserId,
    DateTime DeletedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string SourceModule { get; init; } = "Users";
}
