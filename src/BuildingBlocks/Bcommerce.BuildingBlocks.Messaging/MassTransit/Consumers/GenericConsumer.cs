using Bcommerce.BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Consumers;

// Exemplo de consumer genérico se necessário, 
// mas geralmente os consumers são específicos por módulo.

/// <summary>
/// Consumidor genérico para eventos de integração.
/// </summary>
/// <typeparam name="TEvent">Tipo do evento a ser consumido.</typeparam>
/// <remarks>
/// Adaptador entre MassTransit e IIntegrationEventHandler.
/// - Encaminha a mensagem para o handler da aplicação
/// - Gerencia logging básico
/// 
/// Exemplo de uso:
/// <code>
/// services.AddConsumer&lt;GenericConsumer&lt;MyEvent&gt;&gt;();
/// </code>
/// </remarks>
public class GenericConsumer<TEvent>(IIntegrationEventHandler<TEvent> handler, ILogger<GenericConsumer<TEvent>> logger) : IConsumer<TEvent>
    where TEvent : class, Application.Abstractions.Messaging.IIntegrationEvent
{
    private readonly IIntegrationEventHandler<TEvent> _handler = handler;
    private readonly ILogger<GenericConsumer<TEvent>> _logger = logger;

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        _logger.LogInformation("Consumindo evento genérico: {EventType}", typeof(TEvent).Name);
        await _handler.Handle(context.Message, context.CancellationToken);
    }
}
