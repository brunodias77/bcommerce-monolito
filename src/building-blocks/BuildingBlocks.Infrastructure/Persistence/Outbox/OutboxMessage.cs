namespace BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>
/// Representa uma mensagem de evento de domínio armazenada na tabela de outbox (shared.domain_events)
/// Implementa o padrão Transactional Outbox para garantir consistência eventual
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Identificador único da mensagem
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nome do módulo que gerou o evento (users, catalog, cart, orders, payments, coupons)
    /// </summary>
    public string Module { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo do agregado que gerou o evento (ex: "Product", "Order", "User")
    /// </summary>
    public string AggregateType { get; private set; } = string.Empty;

    /// <summary>
    /// ID do agregado que gerou o evento
    /// </summary>
    public Guid AggregateId { get; private set; }

    /// <summary>
    /// Tipo do evento (nome completo da classe do evento)
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// Payload JSON do evento serializado
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>
    /// Data e hora de criação do evento
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Data e hora em que o evento foi processado (null se ainda não foi processado)
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Mensagem de erro caso o processamento tenha falho
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Número de tentativas de processamento
    /// </summary>
    public int RetryCount { get; private set; }

    // Construtor privado para EF Core
    private OutboxMessage()
    {
    }

    /// <summary>
    /// Cria uma nova mensagem de outbox
    /// </summary>
    /// <param name="module">Módulo de origem</param>
    /// <param name="aggregateType">Tipo do agregado</param>
    /// <param name="aggregateId">ID do agregado</param>
    /// <param name="eventType">Tipo do evento</param>
    /// <param name="payload">Payload JSON serializado</param>
    public OutboxMessage(
        string module,
        string aggregateType,
        Guid aggregateId,
        string eventType,
        string payload)
    {
        Id = Guid.NewGuid();
        Module = module;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        EventType = eventType;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    /// <summary>
    /// Marca a mensagem como processada com sucesso
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Registra uma falha no processamento
    /// </summary>
    /// <param name="errorMessage">Mensagem de erro</param>
    public void MarkAsFailed(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Verifica se a mensagem deve ser reprocessada
    /// </summary>
    /// <param name="maxRetries">Número máximo de tentativas</param>
    /// <returns>True se deve tentar novamente</returns>
    public bool ShouldRetry(int maxRetries = 3)
    {
        return ProcessedAt == null && RetryCount < maxRetries;
    }
}
