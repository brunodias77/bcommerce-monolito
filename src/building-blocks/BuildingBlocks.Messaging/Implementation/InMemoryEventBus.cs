using System.Collections.Concurrent;
using BuildingBlocks.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Implementation;

/// <summary>
/// Implementação em memória do Event Bus para desenvolvimento e testes
///
/// Características:
/// - Não requer infraestrutura externa (RabbitMQ, etc.)
/// - Processamento síncrono na mesma thread
/// - Sem persistência (eventos são perdidos se a aplicação reiniciar)
/// - Sem retry automático
/// - Ideal para: desenvolvimento local, testes de integração, CI/CD
///
/// Limitações:
/// - Não é adequado para produção
/// - Não sobrevive a reinicializações
/// - Sem garantias de entrega
/// - Sem ordenação de mensagens
/// - Sem distribuição entre instâncias
///
/// Uso recomendado:
/// - Ambiente de desenvolvimento local
/// - Testes automatizados
/// - Prototipagem rápida
///
/// Para produção, use MassTransitEventBus com RabbitMQ
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    // Dicionário thread-safe que mapeia tipo de evento → lista de tipos de handlers
    private readonly ConcurrentDictionary<Type, List<Type>> _handlers = new();

    // Lock para garantir thread-safety ao adicionar/remover handlers
    private readonly object _lock = new();

    public InMemoryEventBus(
        IServiceProvider serviceProvider,
        ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Publica um evento para todos os handlers registrados
    /// Processa de forma síncrona e sequencial
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        var eventType = typeof(TEvent);
        var eventName = eventType.Name;

        _logger.LogInformation(
            "Publicando evento em memória: {EventName} (ID: {EventId})",
            eventName,
            @event.EventId);

        // Busca handlers registrados para este tipo de evento
        if (!_handlers.TryGetValue(eventType, out var handlerTypes) || !handlerTypes.Any())
        {
            _logger.LogWarning(
                "Nenhum handler registrado para o evento: {EventName}",
                eventName);
            return;
        }

        _logger.LogDebug(
            "Encontrados {HandlerCount} handler(s) para o evento {EventName}",
            handlerTypes.Count,
            eventName);

        // Processa cada handler sequencialmente
        foreach (var handlerType in handlerTypes)
        {
            try
            {
                // Cria uma nova scope para resolver dependências do handler
                using var scope = _serviceProvider.CreateScope();

                // Resolve o handler do container de DI
                var handler = scope.ServiceProvider.GetService(handlerType);

                if (handler is null)
                {
                    _logger.LogError(
                        "Não foi possível resolver o handler {HandlerType} para o evento {EventName}",
                        handlerType.Name,
                        eventName);
                    continue;
                }

                // Chama o método Handle do handler
                var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<TEvent>.Handle));

                if (handleMethod is null)
                {
                    _logger.LogError(
                        "Método Handle não encontrado no handler {HandlerType}",
                        handlerType.Name);
                    continue;
                }

                _logger.LogDebug(
                    "Executando handler {HandlerType} para o evento {EventName}",
                    handlerType.Name,
                    eventName);

                // Invoca o handler
                var task = (Task)handleMethod.Invoke(handler, new object[] { @event, cancellationToken })!;
                await task;

                _logger.LogDebug(
                    "Handler {HandlerType} executado com sucesso",
                    handlerType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao executar handler {HandlerType} para o evento {EventName}",
                    handlerType.Name,
                    eventName);

                // Em produção (MassTransit), a mensagem seria reprocessada
                // Aqui, apenas logamos o erro e continuamos
            }
        }

        _logger.LogInformation(
            "Evento {EventName} processado por {HandlerCount} handler(s)",
            eventName,
            handlerTypes.Count);
    }

    /// <summary>
    /// Registra um handler para um tipo de evento
    /// </summary>
    public void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Type>();
            }

            // Evita registrar o mesmo handler múltiplas vezes
            if (_handlers[eventType].Contains(handlerType))
            {
                _logger.LogWarning(
                    "Handler {HandlerType} já está registrado para o evento {EventType}",
                    handlerType.Name,
                    eventType.Name);
                return;
            }

            _handlers[eventType].Add(handlerType);

            _logger.LogInformation(
                "Handler {HandlerType} registrado para o evento {EventType}",
                handlerType.Name,
                eventType.Name);
        }
    }

    /// <summary>
    /// Remove um handler de um tipo de evento
    /// </summary>
    public void Unsubscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (!_handlers.TryGetValue(eventType, out var handlerTypes))
            {
                _logger.LogWarning(
                    "Nenhum handler registrado para o evento {EventType}",
                    eventType.Name);
                return;
            }

            handlerTypes.Remove(handlerType);

            _logger.LogInformation(
                "Handler {HandlerType} removido do evento {EventType}",
                handlerType.Name,
                eventType.Name);

            // Remove a entrada do dicionário se não há mais handlers
            if (!handlerTypes.Any())
            {
                _handlers.TryRemove(eventType, out _);
            }
        }
    }
}
