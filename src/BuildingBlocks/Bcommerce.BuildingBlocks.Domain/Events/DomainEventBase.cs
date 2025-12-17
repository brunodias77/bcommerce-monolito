using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Events;

/// <summary>
/// Classe base alternativa para eventos de domínio na pasta Events.
/// </summary>
/// <remarks>
/// Implementação equivalente a Base/DomainEvent.
/// - EventId e OccurredOn podem ser sobrescritos em classes derivadas
/// - Preferível usar Base/DomainEvent para novos eventos
/// - Mantida para compatibilidade
/// 
/// Exemplo de uso:
/// <code>
/// public class ProdutoAtualizadoEvent : DomainEventBase
/// {
///     public Guid ProdutoId { get; init; }
///     public string NovoNome { get; init; }
/// }
/// </code>
/// </remarks>
public abstract class DomainEventBase : IDomainEvent
{
    public Guid EventId { get; protected set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
