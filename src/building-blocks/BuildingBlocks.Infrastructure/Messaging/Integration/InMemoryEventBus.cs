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
/// // Registrar handlers estaticamente (recomendado para módulos)
/// InMemoryEventBus.RegisterHandler&lt;UserCreatedIntegrationEvent, UserCreatedIntegrationEventHandler&gt;();
/// 
/// // Ou via instância
/// var eventBus = app.Services.GetRequiredService&lt;IEventBus&gt;();
/// eventBus.Subscribe&lt;UserCreatedIntegrationEvent, UserCreatedIntegrationEventHandler&gt;();
/// </code>
/// </remarks>
public class InMemoryEventBus : IEventBus
{
    // Registro estático de handlers - permite que módulos registrem handlers durante AddModule()
    private static readonly Dictionary<Type, List<Type>> _staticHandlers = new();
    private static readonly object _lock = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus>? _logger;

    public InMemoryEventBus(
        IServiceProvider serviceProvider,
        ILogger<InMemoryEventBus>? logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Registra um handler estaticamente.
    /// Útil para registrar handlers durante a configuração de DI dos módulos.
    /// </summary>
    public static void RegisterHandler<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (!_staticHandlers.ContainsKey(eventType))
            {
                _staticHandlers[eventType] = new List<Type>();
            }

            if (!_staticHandlers[eventType].Contains(handlerType))
            {
                _staticHandlers[eventType].Add(handlerType);
            }
        }
    }

    /// <summary>
    /// Obtém os handlers registrados para um tipo de evento.
    /// </summary>
    public static IReadOnlyCollection<Type> GetHandlersForEvent(Type eventType)
    {
        lock (_lock)
        {
            if (_staticHandlers.TryGetValue(eventType, out var handlers))
            {
                return handlers.AsReadOnly();
            }
            return Array.Empty<Type>();
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var eventType = typeof(TEvent);

        _logger?.LogDebug(
            "Publishing integration event {EventType} with Id {EventId}",
            eventType.Name,
            @event.EventId);

        List<Type>? handlerTypes;
        lock (_lock)
        {
            _staticHandlers.TryGetValue(eventType, out handlerTypes);
        }

        if (handlerTypes == null || handlerTypes.Count == 0)
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
        // Delega para o registro estático
        RegisterHandler<TEvent, THandler>();

        _logger?.LogInformation(
            "Subscribed {HandlerType} to {EventType}",
            typeof(THandler).Name,
            typeof(TEvent).Name);
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (_staticHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handlerType);

                _logger?.LogInformation(
                    "Unsubscribed {HandlerType} from {EventType}",
                    handlerType.Name,
                    eventType.Name);
            }
        }
    }
}

