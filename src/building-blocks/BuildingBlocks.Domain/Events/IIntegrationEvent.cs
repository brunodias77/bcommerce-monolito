namespace BuildingBlocks.Domain.Events;

/// <summary>
/// Interface para eventos de integração entre módulos.
/// </summary>
/// <remarks>
/// DIFERENÇA ENTRE DOMAIN EVENTS E INTEGRATION EVENTS:
/// 
/// Domain Events (IDomainEvent):
/// - Publicados dentro do mesmo módulo
/// - Processados sincronamente via MediatR
/// - Exemplo: OrderPaidEvent dentro do módulo Orders
/// 
/// Integration Events (IIntegrationEvent):
/// - Comunicam diferentes módulos
/// - Salvos no Outbox (shared.domain_events)
/// - Processados assincronamente
/// - Exemplo: PaymentCapturedIntegrationEvent (Payments → Orders)
/// 
/// Fluxo no seu sistema:
/// 1. Payments captura pagamento → levanta PaymentCapturedEvent (domain)
/// 2. Handler converte para PaymentCapturedIntegrationEvent
/// 3. Salvo em shared.domain_events (Outbox)
/// 4. Background job processa e notifica módulo Orders
/// 5. Orders atualiza status do pedido
/// 
/// Exemplo de uso:
/// <code>
/// // No módulo Payments.Contracts:
/// public record PaymentCapturedIntegrationEvent(
///     Guid PaymentId,
///     Guid OrderId,
///     decimal Amount,
///     DateTime CapturedAt
/// ) : IIntegrationEvent;
/// 
/// // No módulo Payments.Application:
/// internal class PaymentCapturedEventHandler
///     : IDomainEventHandler&lt;PaymentCapturedEvent&gt;
/// {
///     private readonly IEventBus _eventBus;
///     
///     public async Task Handle(PaymentCapturedEvent domainEvent, CancellationToken cancellationToken)
///     {
///         var integrationEvent = new PaymentCapturedIntegrationEvent(
///             domainEvent.PaymentId,
///             domainEvent.OrderId,
///             domainEvent.Amount,
///             DateTime.UtcNow
///         );
///         
///         await _eventBus.PublishAsync(integrationEvent, cancellationToken);
///     }
/// }
/// 
/// // No módulo Orders.Application:
/// internal class PaymentCapturedIntegrationEventHandler
///     : IIntegrationEventHandler&lt;PaymentCapturedIntegrationEvent&gt;
/// {
///     public async Task Handle(PaymentCapturedIntegrationEvent @event, CancellationToken cancellationToken)
///     {
///         // Atualizar status do pedido
///     }
/// }
/// </code>
/// </remarks>
public interface IIntegrationEvent
{
    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Data e hora em que o evento ocorreu (UTC).
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Nome do módulo que originou o evento.
    /// </summary>
    string SourceModule { get; }

    /// <summary>
    /// Tipo do evento (nome da classe).
    /// </summary>
    string EventType => GetType().Name;
}