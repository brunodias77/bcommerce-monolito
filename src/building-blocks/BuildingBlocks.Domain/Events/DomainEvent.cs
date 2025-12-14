namespace BuildingBlocks.Domain.Events;

/// <summary>
/// Classe base para eventos de domínio.
/// </summary>
/// <remarks>
/// Eventos de domínio são publicados internamente no mesmo módulo.
/// Para comunicação entre módulos, converta para Integration Events e salve no Outbox.
///
/// IMPORTANTE: Ao criar um evento de domínio:
/// 1. Implemente a propriedade abstrata AggregateId
/// 2. Adicione o atributo [AggregateType("NomeDoAgregado")]
///
/// Exemplo de uso:
/// <code>
/// [AggregateType("Order")]
/// public class OrderPaidEvent : DomainEvent
/// {
///     public Guid OrderId { get; }
///     public decimal Amount { get; }
///
///     public override Guid AggregateId => OrderId;
///
///     public OrderPaidEvent(Guid orderId, decimal amount)
///     {
///         OrderId = orderId;
///         Amount = amount;
///     }
/// }
/// </code>
/// </remarks>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Data e hora em que o evento ocorreu (UTC).
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// ID do agregado que originou o evento.
    /// </summary>
    /// <remarks>
    /// DEVE ser implementado pela classe concreta do evento.
    /// Retorne o ID da entidade principal (agregado) que gerou este evento.
    ///
    /// Exemplo:
    /// <code>
    /// public class ProductCreatedEvent : DomainEvent
    /// {
    ///     public Guid ProductId { get; }
    ///     public override Guid AggregateId => ProductId;
    /// }
    /// </code>
    /// </remarks>
    public abstract Guid AggregateId { get; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}