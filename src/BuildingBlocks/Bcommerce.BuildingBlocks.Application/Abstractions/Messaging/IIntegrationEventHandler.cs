using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Contrato para consumidores de eventos de integração.
/// </summary>
/// <typeparam name="TIntegrationEvent">Tipo do evento a ser consumido.</typeparam>
/// <remarks>
/// Define lógica a ser executada quando um evento externo é recebido.
/// - Implementa INotificationHandler do MediatR
/// - Deve ser idempotente (tratar mensagens duplicadas)
/// - Geralmente acionado por um consumer de Message Bus (RabbitMQ/Kafka)
/// 
/// Exemplo de uso:
/// <code>
/// public class EnviarNFHandler : IIntegrationEventHandler&lt;PedidoPagoIntegrationEvent&gt;
/// {
///     public async Task Handle(PedidoPagoIntegrationEvent evento, CancellationToken ct)
///     {
///         // Emitir Nota Fiscal
///     }
/// }
/// </code>
/// </remarks>
public interface IIntegrationEventHandler<in TIntegrationEvent> : INotificationHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
}
