using BuildingBlocks.Domain.Events;

namespace Users.Contracts.Events;

/// <summary>
/// Evento de integração publicado quando um novo usuário é criado.
/// Outros módulos podem reagir a este evento (ex: Cart cria carrinho vazio).
/// </summary>
public record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    string UserName,
    string? FirstName,
    string? LastName,
    DateTime CreatedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string SourceModule { get; init; } = "Users";
}
