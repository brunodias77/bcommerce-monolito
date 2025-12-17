namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando um pagamento é confirmado com sucesso.
/// </summary>
/// <remarks>
/// Autoriza a continuidade do fluxo do pedido.
/// - Dispara liberação para envio (Shipping)
/// - Confirma a baixa no estoque (se reservado)
/// 
/// Exemplo de uso:
/// <code>
/// new PaymentCompletedEvent(paymentId, orderId, amount);
/// </code>
/// </remarks>
public record PaymentCompletedEvent(Guid PaymentId, Guid OrderId, decimal Amount) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
