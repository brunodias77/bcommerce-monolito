namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;

/// <summary>
/// Representa uma mensagem recebida para processamento assíncrono (Inbox Pattern).
/// </summary>
/// <remarks>
/// Armazena eventos de integração recebidos para garantir processamento confiável.
/// - Garante idempotência no recebimento de mensagens
/// - Permite reprocessamento em caso de falhas
/// - Mantém histórico de erros e tentativas
/// 
/// Exemplo de uso:
/// <code>
/// var msg = new InboxMessage 
/// { 
///     Type = "PedidoCriadoEvent", 
///     Content = "{...}", 
///     OccurredOnUtc = DateTime.UtcNow 
/// };
/// </code>
/// </remarks>
public class InboxMessage
{
    /// <summary>Identificador único da mensagem.</summary>
    public Guid Id { get; set; }
    /// <summary>Nome completo do tipo do evento (Assembly Qualified Name).</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Conteúdo JSON do evento.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Data de ocorrência do evento na origem.</summary>
    public DateTime OccurredOnUtc { get; set; }
    /// <summary>Data e hora em que foi processado com sucesso (null se pendente).</summary>
    public DateTime? ProcessedOnUtc { get; set; }
    /// <summary>Mensagem de erro caso o processamento tenha falhado.</summary>
    public string? Error { get; set; }
}
