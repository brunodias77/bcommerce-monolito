using BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Implementation;

/// <summary>
/// Implementação do Event Bus usando MassTransit com RabbitMQ
///
/// MassTransit é um framework robusto de mensageria que fornece:
/// - Integração com RabbitMQ, Azure Service Bus, Amazon SQS, etc.
/// - Retry automático com exponential backoff
/// - Dead letter queues para mensagens que falharam
/// - Idempotência e deduplicação de mensagens
/// - Monitoramento e observabilidade
/// - Serialização automática (JSON)
/// - Roteamento baseado em tipo de mensagem
///
/// Arquitetura de filas no RabbitMQ:
///
/// Para cada evento de integração, são criadas:
///
/// 1. Exchange: bcommerce.integration-events
///    - Tipo: topic
///    - Roteia eventos para as filas corretas
///
/// 2. Fila principal: bcommerce.{module}.{event-name}
///    - Exemplo: bcommerce.orders.pedido-criado
///    - Processa eventos normalmente
///
/// 3. Fila de retry: bcommerce.{module}.{event-name}_retry
///    - Mensagens que falharam são reenfileiradas aqui
///    - Tentativas com intervalo crescente (5s, 15s, 25s)
///
/// 4. Fila de erro (dead letter): bcommerce.{module}.{event-name}_error
///    - Mensagens que esgotaram as tentativas
///    - Requer intervenção manual ou reprocessamento
///
/// Exemplo de fluxo baseado no schema SQL:
///
/// Módulo Orders publica PedidoCriadoIntegrationEvent:
/// 1. Evento serializado para JSON
/// 2. Publicado na exchange: bcommerce.integration-events
/// 3. RabbitMQ roteia para fila: bcommerce.payments.pedido-criado
/// 4. Módulo Payments consome da fila
/// 5. Se falhar: vai para retry (até 3 vezes)
/// 6. Se esgotar tentativas: vai para dead letter
///
/// Benefícios sobre InMemoryEventBus:
/// - Persistência: Eventos sobrevivem a reinicializações
/// - Garantias de entrega: At-least-once delivery
/// - Resiliência: Retry automático
/// - Escalabilidade: Múltiplas instâncias podem consumir
/// - Observabilidade: Métricas e monitoramento
/// - Desacoplamento temporal: Publisher e consumer não precisam estar online simultaneamente
/// </summary>
public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventBus> _logger;

    public MassTransitEventBus(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitEventBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Publica um evento de integração no RabbitMQ
    /// O MassTransit cuida de:
    /// - Serializar para JSON
    /// - Rotear para as exchanges corretas
    /// - Adicionar metadados (timestamp, correlationId, etc.)
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        var eventType = typeof(TEvent);
        var eventName = eventType.Name;

        try
        {
            _logger.LogInformation(
                "Publicando evento de integração: {EventName} (ID: {EventId}) no RabbitMQ",
                eventName,
                @event.EventId);

            // Publica o evento no RabbitMQ via MassTransit
            await _publishEndpoint.Publish(@event, cancellationToken);

            _logger.LogInformation(
                "Evento {EventName} publicado com sucesso no RabbitMQ",
                eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao publicar evento {EventName} (ID: {EventId}) no RabbitMQ",
                eventName,
                @event.EventId);

            throw;
        }
    }

    /// <summary>
    /// Registra um consumer no MassTransit
    /// Nota: Em MassTransit, a assinatura é feita via configuração no DependencyInjection
    /// Este método existe apenas para manter compatibilidade com a interface
    /// </summary>
    public void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _logger.LogWarning(
            "Subscribe chamado manualmente para {EventType} e {HandlerType}. " +
            "No MassTransit, consumers devem ser registrados via configuração no DependencyInjection.",
            typeof(TEvent).Name,
            typeof(THandler).Name);

        // Em MassTransit, a configuração de consumers é feita assim:
        // services.AddMassTransit(x =>
        // {
        //     x.AddConsumer<PedidoCriadoConsumer>();
        //     x.UsingRabbitMq((context, cfg) =>
        //     {
        //         cfg.ReceiveEndpoint("bcommerce.orders.pedido-criado", e =>
        //         {
        //             e.ConfigureConsumer<PedidoCriadoConsumer>(context);
        //         });
        //     });
        // });
    }

    /// <summary>
    /// Remove um consumer do MassTransit
    /// Nota: Em MassTransit, isso requer parar e reconfigurar o bus
    /// Este método existe apenas para manter compatibilidade com a interface
    /// </summary>
    public void Unsubscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _logger.LogWarning(
            "Unsubscribe chamado para {EventType} e {HandlerType}. " +
            "No MassTransit, consumers não podem ser removidos dinamicamente. " +
            "É necessário reconfigurar o bus.",
            typeof(TEvent).Name,
            typeof(THandler).Name);
    }
}

/// <summary>
/// Adapter para integrar IIntegrationEventHandler com MassTransit IConsumer
/// MassTransit espera IConsumer<T>, mas nosso domínio usa IIntegrationEventHandler<T>
/// Este adapter faz a ponte entre os dois
/// </summary>
/// <typeparam name="TEvent">Tipo do evento</typeparam>
/// <typeparam name="THandler">Tipo do handler do domínio</typeparam>
public sealed class MassTransitConsumerAdapter<TEvent, THandler> : IConsumer<TEvent>
    where TEvent : class, IIntegrationEvent
    where THandler : IIntegrationEventHandler<TEvent>
{
    private readonly THandler _handler;
    private readonly ILogger<MassTransitConsumerAdapter<TEvent, THandler>> _logger;

    public MassTransitConsumerAdapter(
        THandler handler,
        ILogger<MassTransitConsumerAdapter<TEvent, THandler>> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    /// <summary>
    /// Método chamado pelo MassTransit quando uma mensagem chega
    /// Delega para o handler do domínio
    /// </summary>
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var eventName = typeof(TEvent).Name;
        var @event = context.Message;

        _logger.LogInformation(
            "Consumindo evento {EventName} (ID: {EventId}) via MassTransit",
            eventName,
            @event.EventId);

        try
        {
            // Delega para o handler do domínio
            await _handler.Handle(@event, context.CancellationToken);

            _logger.LogInformation(
                "Evento {EventName} (ID: {EventId}) processado com sucesso",
                eventName,
                @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar evento {EventName} (ID: {EventId}). " +
                "A mensagem será reenfileirada para retry.",
                eventName,
                @event.EventId);

            // Lança a exceção para que o MassTransit possa aplicar retry
            throw;
        }
    }
}
