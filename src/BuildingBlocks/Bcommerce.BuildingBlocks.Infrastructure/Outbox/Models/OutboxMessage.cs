namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;

/// <summary>
/// Representa uma mensagem de saída para envio assíncrono (Outbox Pattern).
/// </summary>
/// <remarks>
/// Armazena eventos de domínio que devem ser publicados após a transação ser comitada.
/// - Garante atomicidade entre operação de banco e evento (Transactional Outbox)
/// - Processada por um job em background
/// - Evita perda de eventos em caso de falha no broker
/// 
/// Exemplo de uso:
/// <code>
/// var msg = new OutboxMessage 
/// { 
///     Type = "PedidoCriadoEvent", 
///     Content = json, 
///     OccurredOnUtc = DateTime.UtcNow 
/// };
/// </code>
/// </remarks>
public class OutboxMessage
{
    /// <summary>Identificador único da mensagem.</summary>
    public Guid Id { get; set; }
    /// <summary>Nome completo do tipo do evento.</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Conteúdo serializado do evento.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Data de ocorrência do evento.</summary>
    public DateTime OccurredOnUtc { get; set; }
    /// <summary>Data e hora do processamento (null se pendente).</summary>
    public DateTime? ProcessedOnUtc { get; set; }
    /// <summary>Mensagem de erro em caso de falha no envio.</summary>
    public string? Error { get; set; }
}
