namespace BuildingBlocks.Domain.Events;

/// <summary>
/// Classe base para eventos de integração entre módulos.
/// </summary>
/// <remarks>
/// Usar records C# é recomendado para Integration Events por serem imutáveis e terem igualdade estrutural.
/// 
/// Exemplo preferencial (usando record):
/// <code>
/// public record OrderCreatedIntegrationEvent(
///     Guid OrderId,
///     Guid UserId,
///     decimal TotalAmount,
///     string OrderNumber
/// ) : IntegrationEvent("orders");
/// </code>
/// 
/// Exemplo usando classe (caso precise de lógica adicional):
/// <code>
/// public class OrderCreatedIntegrationEvent : IntegrationEvent
/// {
///     public Guid OrderId { get; }
///     public Guid UserId { get; }
///     public decimal TotalAmount { get; }
///     public string OrderNumber { get; }
///     
///     public OrderCreatedIntegrationEvent(
///         Guid orderId,
///         Guid userId,
///         decimal totalAmount,
///         string orderNumber) : base("orders")
///     {
///         OrderId = orderId;
///         UserId = userId;
///         TotalAmount = totalAmount;
///         OrderNumber = orderNumber;
///     }
/// }
/// </code>
/// </remarks>
public abstract class IntegrationEvent : IIntegrationEvent
{
    /// <summary>
    /// Identificador único do evento.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Data e hora em que o evento ocorreu (UTC).
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Nome do módulo que originou o evento.
    /// Valores possíveis: "users", "catalog", "cart", "orders", "payments", "coupons"
    /// </summary>
    public string SourceModule { get; }

    /// <summary>
    /// Tipo do evento (nome da classe).
    /// </summary>
    public string EventType => GetType().Name;

    protected IntegrationEvent(string sourceModule)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SourceModule = sourceModule ?? throw new ArgumentNullException(nameof(sourceModule));
    }

    /// <summary>
    /// Construtor para deserialização.
    /// </summary>
    protected IntegrationEvent(Guid eventId, DateTime occurredOn, string sourceModule)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
        SourceModule = sourceModule ?? throw new ArgumentNullException(nameof(sourceModule));
    }
}