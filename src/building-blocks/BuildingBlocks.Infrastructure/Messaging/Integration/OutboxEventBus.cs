using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BuildingBlocks.Infrastructure.Messaging.Integration;

/// <summary>
/// Event Bus transacional usando o padrão Outbox.
/// </summary>
/// <remarks>
/// <strong>Garantia de Atomicidade:</strong>
/// Ao invés de enviar o evento imediatamente para um Broker (o que poderia falhar após o commit do banco),
/// este Bus salva o evento na tabela `outbox_messages` dentro da MESMA transação do negócio.
/// 
/// <strong>Fluxo:</strong>
/// 1. Negócio chama `PublishAsync`.
/// 2. Evento é serializado e adicionado ao DbContext (State = Added).
/// 3. Negócio chama `SaveChangesAsync`.
/// 4. O banco grava (Atomic Commit) as alterações de negócio E o evento no Outbox.
/// 
/// <strong>Processamento Posterior:</strong>
/// Um Job em background (ProcessOutboxMessagesJob) lê a tabela e despacha o evento.
/// </remarks>
public class OutboxEventBus : IEventBus
{
    private readonly DbContext _dbContext;
    private readonly string _moduleName;
    private readonly ILogger<OutboxEventBus>? _logger;

    // Handlers são registrados para o ProcessOutboxMessagesJob
    private static readonly Dictionary<Type, List<Type>> _handlers = new();

    public OutboxEventBus(
        DbContext dbContext,
        string moduleName,
        ILogger<OutboxEventBus>? logger = null)
    {
        _dbContext = dbContext;
        _moduleName = moduleName;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var outboxMessage = new OutboxMessage
        {
            Id = @event.EventId,
            Module = _moduleName,
            AggregateType = GetAggregateType(@event),
            AggregateId = @event.EventId, // Use EventId as aggregate identifier for integration events
            EventType = @event.GetType().AssemblyQualifiedName ?? @event.GetType().FullName ?? @event.GetType().Name,
            Payload = SerializeEvent(@event),
            CreatedAt = @event.OccurredOn
        };

        await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);

        _logger?.LogDebug(
            "Integration event {EventType} saved to outbox with Id {EventId}",
            @event.GetType().Name,
            @event.EventId);

        // Nota: O evento será salvo quando SaveChangesAsync for chamado
        // Isso garante atomicidade com outras operações na mesma transação
    }

    public async Task PublishManyAsync(
        IEnumerable<IIntegrationEvent> events,
        CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = @event.EventId,
                Module = _moduleName,
                AggregateType = GetAggregateType(@event),
                AggregateId = @event.EventId, // Use EventId as aggregate identifier for integration events
                EventType = @event.GetType().AssemblyQualifiedName ?? @event.GetType().FullName ?? @event.GetType().Name,
                Payload = SerializeEvent(@event),
                CreatedAt = @event.OccurredOn
            };

            await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
        }

        _logger?.LogDebug(
            "Saved {Count} integration events to outbox",
            events.Count());
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_handlers)
        {
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Type>();
            }

            if (!_handlers[eventType].Contains(handlerType))
            {
                _handlers[eventType].Add(handlerType);
            }
        }

        _logger?.LogInformation(
            "Registered handler {HandlerType} for event {EventType}",
            handlerType.Name,
            eventType.Name);
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_handlers)
        {
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handlerType);
            }
        }

        _logger?.LogInformation(
            "Unregistered handler {HandlerType} for event {EventType}",
            handlerType.Name,
            eventType.Name);
    }

    /// <summary>
    /// Obtém os handlers registrados para um tipo de evento.
    /// Usado pelo ProcessOutboxMessagesJob.
    /// </summary>
    public static IReadOnlyList<Type> GetHandlersForEvent(Type eventType)
    {
        lock (_handlers)
        {
            return _handlers.TryGetValue(eventType, out var handlers)
                ? handlers.AsReadOnly()
                : Array.Empty<Type>();
        }
    }

    /// <summary>
    /// Obtém todos os tipos de eventos registrados.
    /// </summary>
    public static IReadOnlyCollection<Type> GetRegisteredEventTypes()
    {
        lock (_handlers)
        {
            return _handlers.Keys.ToList().AsReadOnly();
        }
    }

    private static string GetAggregateType(IIntegrationEvent @event)
    {
        // Tenta obter do próprio evento
        var typeName = @event.GetType().Name;

        // Remove sufixo "IntegrationEvent"
        if (typeName.EndsWith("IntegrationEvent"))
            typeName = typeName[..^16];

        // Tenta extrair o nome do agregado
        var commonSuffixes = new[] { "Created", "Updated", "Deleted", "Changed" };
        foreach (var suffix in commonSuffixes)
        {
            if (typeName.EndsWith(suffix))
                return typeName[..^suffix.Length];
        }

        return typeName;
    }

    private static string SerializeEvent(IIntegrationEvent @event)
    {
        // Importante: TypeNameHandling.All é necessário para que, ao deserializar,
        // o Json.NET saiba exatamente qual classe instanciar.
        // Isso é crucial pois o campo 'EventType' na tabela pode não ter o assembly qualified name completo
        // dependendo de como foi gravado ou migrado.
        return JsonConvert.SerializeObject(@event, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    }
}

/// <summary>
/// Factory para criar OutboxEventBus com o módulo correto.
/// </summary>
public class OutboxEventBusFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxEventBus>? _logger;

    public OutboxEventBusFactory(
        IServiceProvider serviceProvider,
        ILogger<OutboxEventBus>? logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public OutboxEventBus Create(DbContext dbContext, string moduleName)
    {
        return new OutboxEventBus(dbContext, moduleName, _logger);
    }
}
