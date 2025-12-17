namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando um carrinho é convertido em pedido.
/// </summary>
/// <remarks>
/// Sinaliza o sucesso do checkout.
/// - Notifica outros serviços sobre a criação de um pedido a partir de um carrinho
/// - Usado para limpeza de carrinho ou analytics
/// 
/// Exemplo de uso:
/// <code>
/// var evento = new CartConvertedEvent(cartId, orderId, userId);
/// </code>
/// </remarks>
public record CartConvertedEvent(Guid CartId, Guid OrderId, Guid UserId) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
