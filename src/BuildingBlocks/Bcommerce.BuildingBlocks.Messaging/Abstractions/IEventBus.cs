namespace Bcommerce.BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Abstração para publicação de mensagens no barramento de eventos (Message Broker).
/// </summary>
/// <remarks>
/// Define o contrato para envio assíncrono de mensagens.
/// - Desacopla a aplicação da implementação específica do broker (RabbitMQ, Azure Service Bus)
/// - Suporta publicação de qualquer objeto como mensagem
/// 
/// Exemplo de uso:
/// <code>
/// await _eventBus.PublishAsync(new OrderPlacedEvent(id), cancellationsToken);
/// </code>
/// </remarks>
public interface IEventBus
{
    /// <summary>
    /// Publica uma mensagem no barramento de eventos.
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem.</typeparam>
    /// <param name="message">Objeto da mensagem a ser enviada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class;
}
