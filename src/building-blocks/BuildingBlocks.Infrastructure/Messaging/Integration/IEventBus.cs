using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Infrastructure.Messaging.Integration;

/// <summary>
/// Interface para o barramento de eventos de integração (Event Bus).
/// Responsável pela comunicação assíncrona entre módulos (Bounded Contexts) diferentes.
/// </summary>
/// <remarks>
/// <strong>Domain Events vs Integration Events:</strong>
/// 1. <strong>Domain Events (MediatR):</strong>
///    - Ocorrem DENTRO de um mesmo módulo/transação.
///    - Síncronos (geralmente).
///    - Ex: 'OrderCreatedDomainEvent' dispara 'DecreaseStockHandler' (no mesmo DbContext).
/// 
/// 2. <strong>Integration Events (IEventBus):</strong>
///    - Ocorrem ENTRE módulos diferentes.
///    - Assíncronos (Eventual Consistency).
///    - Ex: 'OrderPaymentConfirmed' (Pagamentos) dispara 'ShipOrder' (Entregas).
/// 
/// <strong>Pub/Sub Pattern:</strong>
/// Permite desacoplamento total. O publicador não conhece os assinantes.
/// </remarks>
public interface IEventBus
{
    /// <summary>
    /// Publica um Integration Event de forma assíncrona.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <param name="event">Evento a ser publicado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;

    /// <summary>
    /// Publica múltiplos Integration Events em lote.
    /// </summary>
    /// <remarks>
    /// Útil para performance ao processar grandes volumes de eventos.
    /// Em implementações com Outbox, isso permite salvar múltiplos eventos em uma única operação de banco.
    /// </remarks>
    /// <param name="events">Lista de eventos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task PublishManyAsync(IEnumerable<IIntegrationEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um handler para um tipo de evento.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <typeparam name="THandler">Tipo do handler</typeparam>
    void Subscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;

    /// <summary>
    /// Remove a assinatura de um handler para um tipo de evento.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <typeparam name="THandler">Tipo do handler</typeparam>
    void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
}

/// <summary>
/// Interface para handlers de Integration Events.
/// </summary>
/// <typeparam name="TEvent">Tipo do evento</typeparam>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    /// <summary>
    /// Processa o evento.
    /// </summary>
    /// <param name="event">Evento a ser processado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
