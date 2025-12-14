using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Infrastructure.Messaging.Integration;

/// <summary>
/// Interface para publicação de Integration Events entre módulos.
/// </summary>
/// <remarks>
/// Integration Events vs Domain Events:
/// 
/// Domain Events (IDomainEvent):
/// - Internos ao módulo
/// - Processados sincronamente via MediatR
/// - Exemplo: ProductCreatedEvent dentro de Catalog
/// 
/// Integration Events (IIntegrationEvent):
/// - Entre módulos diferentes
/// - Salvos no Outbox (shared.domain_events)
/// - Processados assincronamente
/// - Exemplo: PaymentCapturedIntegrationEvent (Payments → Orders)
/// 
/// Fluxo típico:
/// 1. Handler de Domain Event converte para Integration Event
/// 2. Publica via IEventBus
/// 3. Salvo no Outbox (mesma transação)
/// 4. OutboxProcessor processa
/// 5. Handlers de outros módulos recebem
/// 
/// Exemplo de uso:
/// <code>
/// // No módulo Payments
/// internal class PaymentCapturedEventHandler 
///     : INotificationHandler&lt;PaymentCapturedEvent&gt;
/// {
///     private readonly IEventBus _eventBus;
///     
///     public async Task Handle(PaymentCapturedEvent domainEvent, CancellationToken ct)
///     {
///         // Converter para Integration Event
///         var integrationEvent = new PaymentCapturedIntegrationEvent(
///             domainEvent.PaymentId,
///             domainEvent.OrderId,
///             domainEvent.Amount,
///             DateTime.UtcNow
///         );
///         
///         // Publicar (salva no Outbox)
///         await _eventBus.PublishAsync(integrationEvent, ct);
///     }
/// }
/// 
/// // No módulo Orders
/// internal class PaymentCapturedIntegrationEventHandler
///     : INotificationHandler&lt;PaymentCapturedIntegrationEvent&gt;
/// {
///     public async Task Handle(PaymentCapturedIntegrationEvent @event, CancellationToken ct)
///     {
///         // Atualizar pedido
///         var order = await _repository.GetByIdAsync(@event.OrderId);
///         order.MarkAsPaid(@event.CapturedAt);
///         await _unitOfWork.SaveChangesAsync(ct);
///     }
/// }
/// </code>
/// </remarks>
public interface IEventBus
{
    /// <summary>
    /// Publica um Integration Event no Outbox.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <param name="event">Evento a ser publicado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;

    /// <summary>
    /// Publica múltiplos Integration Events no Outbox.
    /// </summary>
    Task PublishAsync(
        IEnumerable<IIntegrationEvent> events,
        CancellationToken cancellationToken = default);
}
