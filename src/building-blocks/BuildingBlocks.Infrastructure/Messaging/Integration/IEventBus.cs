using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Infrastructure.Messaging.Integration;

/// <summary>
/// Interface para o Event Bus que publica Integration Events entre módulos.
/// </summary>
/// <remarks>
/// No monolito modular, Integration Events são usados para comunicação entre módulos:
/// 
/// - Domain Events: Internos ao módulo, processados de forma síncrona via MediatR
/// - Integration Events: Entre módulos, processados de forma assíncrona via Outbox/Event Bus
/// 
/// Exemplos de uso:
/// <code>
/// // Publicar um evento
/// await eventBus.PublishAsync(new UserCreatedIntegrationEvent(userId, email));
/// 
/// // Assinar eventos (configurado no Startup)
/// eventBus.Subscribe&lt;UserCreatedIntegrationEvent, UserCreatedIntegrationEventHandler&gt;();
/// </code>
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
    /// Publica múltiplos Integration Events de forma assíncrona.
    /// </summary>
    /// <param name="events">Eventos a serem publicados</param>
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
