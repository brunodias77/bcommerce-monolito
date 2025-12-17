namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando há falha no processamento do pagamento.
/// </summary>
/// <remarks>
/// Indica que o pedido não pode prosseguir.
/// - Gatilho para cancelamento do pedido ou tentativa de nova cobrança
/// - Notifica o usuário sobre o problema
/// 
/// Exemplo de uso:
/// <code>
/// new PaymentFailedEvent(paymentId, orderId, "Insuficient Funds");
/// </code>
/// </remarks>
public record PaymentFailedEvent(Guid PaymentId, Guid OrderId, string Reason) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
