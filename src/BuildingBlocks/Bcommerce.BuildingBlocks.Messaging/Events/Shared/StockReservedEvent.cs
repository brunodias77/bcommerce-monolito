namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando itens são reservados no estoque com sucesso.
/// </summary>
/// <remarks>
/// Confirma a disponibilidade dos produtos para um pedido.
/// - Ocorre antes do pagamento (geralmente)
/// - Bloqueia os itens para impedir venda duplicada
/// 
/// Exemplo de uso:
/// <code>
/// new StockReservedEvent(orderId, productId, qty);
/// </code>
/// </remarks>
public record StockReservedEvent(Guid OrderId, Guid ProductId, int Quantity) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
