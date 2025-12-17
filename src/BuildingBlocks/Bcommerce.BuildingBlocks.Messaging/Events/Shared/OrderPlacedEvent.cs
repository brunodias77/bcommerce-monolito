namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando um novo pedido é realizado.
/// </summary>
/// <remarks>
/// Um dos eventos principais do e-commerce.
/// - Gatilho para reserva de estoque, pagamento e notificações
/// - Contém dados resilidos do pedido para processamento assíncrono
/// 
/// Exemplo de uso:
/// <code>
/// bus.Publish(new OrderPlacedEvent(order.Id, user.Id, order.Total));
/// </code>
/// </remarks>
public record OrderPlacedEvent(Guid OrderId, Guid UserId, decimal TotalAmount) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
