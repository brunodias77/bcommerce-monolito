using BuildingBlocks.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Messaging.Integration;

/// <summary>
/// Implementação in-memory do Event Bus (Barramento de Eventos Local).
/// </summary>
/// <remarks>
/// <strong>Arquitetura de Desenvolvimento:</strong>
/// - Processa eventos na mesma Thread/Task (ou threads do Pool local).
/// - <strong>Sem Persistência:</strong> Se a aplicação cair antes de processar, o evento é PERDIDO.
/// 
/// <strong>Casos de Uso:</strong>
/// - Ambientes de Desenvolvimento (Dev/Local).
/// - Testes unitários/integração.
/// - Cenários onde a perda de eventos é tolerável.
/// 
/// <strong>Estratégia de Handlers Estáticos:</strong>
/// Usa um dicionário estático (_staticHandlers) para permitir que os módulos registrem seus interesses (Subscribe)
/// durante a inicialização (AddModule), mesmo antes do container de DI estar totalmente construído.
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
                // CRÍTICO: Resolve o handler dentro de um novo escopo.
                // Isso permite que o handler use DbContexts e outros serviços Scoped
                // sem conflitar com o escopo original da requisição (se houver).
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

                // Em produção real, exceções aqui parariam o processamento dos próximos handlers.
                // No InMemory, estamos relançando para dar visibilidade do erro.
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

