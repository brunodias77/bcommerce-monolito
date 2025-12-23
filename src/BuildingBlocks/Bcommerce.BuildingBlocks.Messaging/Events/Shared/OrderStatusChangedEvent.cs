namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando o status de um pedido é alterado.
/// </summary>
/// <remarks>
/// Usado para rastreabilidade e atualizações de UI/Notificações.
/// - Informa mudança de estado (ex: Aguardando -> Pago -> Enviado)
/// - Permite que outros domínios reajam a mudanças no ciclo de vida do pedido
/// 
/// Exemplo de uso:
/// <code>
/// new OrderStatusChangedEvent(orderId, "Shipped", "Paid");
/// </code>
/// </remarks>
public record OrderStatusChangedEvent(Guid OrderId, string NewStatus, string OldStatus) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
