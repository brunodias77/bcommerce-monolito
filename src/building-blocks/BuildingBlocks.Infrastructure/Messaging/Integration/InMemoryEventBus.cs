using BuildingBlocks.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Messaging.Integration;

/// <summary>
/// Implementação in-memory do Event Bus.
/// </summary>
/// <remarks>
/// Esta implementação é adequada para:
/// - Desenvolvimento local
/// - Testes de integração
/// - Monolitos simples onde todos os handlers estão no mesmo processo
/// 
/// Para produção em ambiente distribuído, use uma implementação baseada em
/// message broker (RabbitMQ, Azure Service Bus, etc.) ou o OutboxEventBus.
/// 
/// Configuração:
/// <code>
/// services.AddSingleton&lt;IEventBus, InMemoryEventBus&gt;();
/// 
/// // Registrar handlers
/// services.AddScoped&lt;UserCreatedIntegrationEventHandler&gt;();
/// 
/// // Configurar assinaturas
/// var eventBus = app.Services.GetRequiredService&lt;IEventBus&gt;();
/// eventBus.Subscribe&lt;UserCreatedIntegrationEvent, UserCreatedIntegrationEventHandler&gt;();
/// </code>
/// </remarks>
public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Type>> _handlers = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus>? _logger;

    public InMemoryEventBus(
        IServiceProvider serviceProvider,
        ILogger<InMemoryEventBus>? logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var eventType = typeof(TEvent);

        _logger?.LogDebug(
            "Publishing integration event {EventType} with Id {EventId}",
            eventType.Name,
            @event.EventId);

        if (!_handlers.TryGetValue(eventType, out var handlerTypes))
        {
            _logger?.LogWarning("No handlers registered for event {EventType}", eventType.Name);
            return;
        }

        using var scope = _serviceProvider.CreateScope();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                var handler = scope.ServiceProvider.GetService(handlerType);

                if (handler == null)
                {
                    _logger?.LogWarning(
                        "Handler {HandlerType} not found in DI container",
                        handlerType.Name);
                    continue;
                }

                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = (Task?)handleMethod.Invoke(handler, new object[] { @event, cancellationToken });
                    if (task != null)
                    {
                        await task;
                    }
                }

                _logger?.LogDebug(
                    "Event {EventType} handled by {HandlerType}",
                    eventType.Name,
                    handlerType.Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error handling event {EventType} with handler {HandlerType}",
                    eventType.Name,
                    handlerType.Name);

                // Em produção, considere implementar retry ou dead letter queue
                throw;
            }
        }
    }

    public async Task PublishManyAsync(
        IEnumerable<IIntegrationEvent> events,
        CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            // Usa reflection para chamar PublishAsync<TEvent>
            var method = typeof(InMemoryEventBus)
                .GetMethod(nameof(PublishAsync))!
                .MakeGenericMethod(@event.GetType());

            var task = (Task?)method.Invoke(this, new object[] { @event, cancellationToken });
            if (task != null)
            {
                await task;
            }
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Type>();
        }

        if (!_handlers[eventType].Contains(handlerType))
        {
            _handlers[eventType].Add(handlerType);

            _logger?.LogInformation(
                "Subscribed {HandlerType} to {EventType}",
                handlerType.Name,
                eventType.Name);
        }
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handlerType);

            _logger?.LogInformation(
                "Unsubscribed {HandlerType} from {EventType}",
                handlerType.Name,
                eventType.Name);
        }
    }
}
