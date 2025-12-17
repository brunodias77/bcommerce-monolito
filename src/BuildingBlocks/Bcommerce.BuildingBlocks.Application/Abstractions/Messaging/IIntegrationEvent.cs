using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Contrato para eventos de integração entre microserviços ou módulos.
/// </summary>
/// <remarks>
/// Eventos usados para comunicação assíncrona (pub/sub) via Message Bus.
/// - Deve ser serializável (JSON)
/// - Carrega apenas dados essenciais para o consumidor
/// - Desacopla produtores e consumidores
/// 
/// Exemplo de uso:
/// <code>
/// public record PedidoPagoIntegrationEvent(Guid PedidoId, DateTime DataPagamento) 
///     : IIntegrationEvent;
/// </code>
/// </remarks>
public interface IIntegrationEvent : INotification
{
    /// <summary>Identificador único global do evento.</summary>
    Guid EventId { get; }
    /// <summary>Data de ocorrência do evento em UTC.</summary>
    DateTime OccurredOn { get; }
    /// <summary>Tipo do evento para roteamento e desserialização.</summary>
    string EventType { get; }
}
