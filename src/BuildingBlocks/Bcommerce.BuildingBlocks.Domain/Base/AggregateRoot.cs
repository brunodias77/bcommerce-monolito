using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Base;

/// <summary>
/// Classe base para raizes de agregado no padrão DDD.
/// </summary>
/// <typeparam name="TId">Tipo do identificador.</typeparam>
/// <remarks>
/// Aggregate Root gerencia eventos de domínio e garante consistência.
/// - Coleta eventos durante operações de negócio
/// - Eventos são despachados após SaveChanges
/// - Única entidade do agregado com repositório
/// 
/// Exemplo de uso:
/// <code>
/// public class Pedido : AggregateRoot&lt;Guid&gt;
/// {
///     public void Finalizar()
///     {
///         Status = StatusPedido.Finalizado;
///         AddDomainEvent(new PedidoFinalizadoEvent(Id));
///     }
/// }
/// </code>
/// </remarks>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Adiciona um evento de domínio à coleção.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>Remove um evento específico da coleção.</summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <inheritdoc />
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
