using MediatR;

namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Contrato para eventos de domínio no padrão DDD.
/// </summary>
/// <remarks>
/// Eventos são fatos que ocorreram no domínio e devem ser propagados.
/// - Imutáveis após criação
/// - Publicados via MediatR (INotification)
/// - Usados para comunicação entre agregados
/// 
/// Exemplo de uso:
/// <code>
/// public class PedidoCriadoEvent : DomainEvent
/// {
///     public Guid PedidoId { get; }
///     public decimal ValorTotal { get; }
///     
///     public PedidoCriadoEvent(Guid pedidoId, decimal valorTotal)
///     {
///         PedidoId = pedidoId;
///         ValorTotal = valorTotal;
///     }
/// }
/// </code>
/// </remarks>
public interface IDomainEvent : INotification
{
    /// <summary>Identificador único do evento.</summary>
    Guid EventId { get; }
    /// <summary>Data/hora em que o evento ocorreu (UTC).</summary>
    DateTime OccurredOn { get; }
    /// <summary>Nome do tipo do evento para serialização/log.</summary>
    string EventType { get; }
}
