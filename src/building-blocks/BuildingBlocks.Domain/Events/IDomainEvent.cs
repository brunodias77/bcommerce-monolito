using MediatR;

namespace BuildingBlocks.Domain.Events;

/// <summary>
/// Interface marcadora para eventos de domínio.
/// </summary>
/// <remarks>
/// Implementa INotification do MediatR para publicação via Mediator.
/// Eventos de domínio são publicados internamente no módulo.
/// Para comunicação entre módulos, use Integration Events (salvos no Outbox).
///
/// IMPORTANTE: Todos os domain events devem:
/// 1. Implementar AggregateId para identificar a entidade fonte
/// 2. Usar [AggregateType] attribute para identificar o tipo do agregado
///
/// Exemplo:
/// <code>
/// [AggregateType("Order")]
/// public class OrderPaidEvent : DomainEvent
/// {
///     public Guid OrderId { get; }
///     public override Guid AggregateId => OrderId;
/// }
/// </code>
/// </remarks>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Data e hora em que o evento ocorreu (UTC).
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// ID do agregado que originou o evento.
    /// </summary>
    /// <remarks>
    /// Deve ser implementado pela classe concreta do evento.
    /// Usado pelo Outbox para rastreabilidade.
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
    Guid AggregateId { get; }
}

/// <summary>
/// Atributo para identificar o tipo do agregado em eventos de domínio.
/// </summary>
/// <remarks>
/// Use este atributo para especificar explicitamente o nome do agregado
/// que será salvo na coluna aggregate_type da tabela domain_events.
///
/// Exemplo:
/// <code>
/// [AggregateType("Product")]
/// public class ProductCreatedEvent : DomainEvent { ... }
///
/// [AggregateType("Order")]
/// public class OrderPaidEvent : DomainEvent { ... }
/// </code>
///
/// Se o atributo não for especificado, o sistema usará heurística
/// (remove sufixo "Event" do nome da classe), mas isso é desencorajado.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AggregateTypeAttribute : Attribute
{
    /// <summary>
    /// Nome do tipo do agregado (ex: "Product", "Order", "Payment").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Cria um novo atributo de tipo de agregado.
    /// </summary>
    /// <param name="name">Nome do tipo do agregado</param>
    public AggregateTypeAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Aggregate type name is required", nameof(name));

        Name = name;
    }
}