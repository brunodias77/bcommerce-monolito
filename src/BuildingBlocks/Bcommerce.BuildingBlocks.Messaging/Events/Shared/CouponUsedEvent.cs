namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando um cupom é utilizado com sucesso em um pedido.
/// </summary>
/// <remarks>
/// Sinaliza o decremento de uso ou inativação do cupom.
/// - Processado pelo módulo de Descontos/Marketing
/// - Atualiza contadores de uso do cupom
/// 
/// Exemplo de uso:
/// <code>
/// var evento = new CouponUsedEvent(couponId, orderId, userId);
/// </code>
/// </remarks>
public record CouponUsedEvent(Guid CouponId, Guid OrderId, Guid UserId) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
