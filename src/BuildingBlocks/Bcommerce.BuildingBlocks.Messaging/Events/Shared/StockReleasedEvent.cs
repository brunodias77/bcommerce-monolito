namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando uma reserva de estoque é cancelada/liberada.
/// </summary>
/// <remarks>
/// Devolve itens ao estoque disponível.
/// - Ocorre após cancelamento de pedido ou falha no pagamento
/// - Garante que o estoque não fique "preso"
/// 
/// Exemplo de uso:
/// <code>
/// new StockReleasedEvent(orderId, productId, qty);
/// </code>
/// </remarks>
public record StockReleasedEvent(Guid OrderId, Guid ProductId, int Quantity) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
