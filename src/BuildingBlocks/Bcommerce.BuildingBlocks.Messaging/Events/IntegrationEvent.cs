using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

namespace Bcommerce.BuildingBlocks.Messaging.Events;

/// <summary>
/// Classe base abstrata para todos os eventos de integração.
/// </summary>
/// <remarks>
/// Define a estrutura comum para mensagens trocadas entre serviços.
/// - Contém metadados essenciais (ID, Data, Tipo)
/// - Implementa IIntegrationEvent para tipagem forte
/// 
/// Exemplo de uso:
/// <code>
/// public record PedidoCriadoEvent(...) : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
/// </code>
/// </remarks>
public abstract record IntegrationEvent(Guid EventId, DateTime OccurredOn) : IIntegrationEvent
{
    /// <summary>Nome do tipo do evento, derivado da classe concreta.</summary>
    public string EventType => GetType().Name;
}
