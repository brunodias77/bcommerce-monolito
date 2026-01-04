namespace BuildingBlocks.Infrastructure.Persistence.Inbox;

/// <summary>
/// Representa uma mensagem processada armazenada na tabela inbox (shared.processed_events)
/// Implementa o padrão Inbox para garantir idempotência no processamento de eventos
/// </summary>
public sealed class InboxMessage
{
    /// <summary>
    /// Identificador único da mensagem (mesmo ID do evento original)
    /// Garante que um evento não seja processado mais de uma vez
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Tipo do evento processado (nome completo da classe do evento)
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// Nome do módulo que processou o evento (users, catalog, cart, orders, payments, coupons)
    /// </summary>
    public string Module { get; private set; } = string.Empty;

    /// <summary>
    /// Data e hora em que o evento foi processado
    /// </summary>
    public DateTime ProcessedAt { get; private set; }

    // Construtor privado para EF Core
    private InboxMessage()
    {
    }

    /// <summary>
    /// Cria um novo registro de mensagem processada
    /// </summary>
    /// <param name="id">ID do evento (deve ser o mesmo do evento original)</param>
    /// <param name="eventType">Tipo do evento</param>
    /// <param name="module">Módulo que processou</param>
    public InboxMessage(Guid id, string eventType, string module)
    {
        Id = id;
        EventType = eventType;
        Module = module;
        ProcessedAt = DateTime.UtcNow;
    }
}