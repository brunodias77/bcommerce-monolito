namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models;

/// <summary>
/// Representa um registro de auditoria de alterações em entidades.
/// </summary>
/// <remarks>
/// Armazena o histórico de mudanças de estado no banco de dados.
/// - Captura valores antigos e novos de propriedades modificadas
/// - Rastreia usuário, data e tipo de operação (Insert, Update, Delete)
/// - Utilizado para compliance e rastreabilidade
/// 
/// Exemplo de uso:
/// <code>
/// var log = new AuditLog 
/// { 
///     TableName = "Produtos", 
///     Type = "Update", 
///     OldValues = "{...}", 
///     NewValues = "{...}" 
/// };
/// </code>
/// </remarks>
public class AuditLog
{
    /// <summary>Identificador único do registro de log.</summary>
    public Guid Id { get; set; }
    /// <summary>ID do usuário que realizou a alteração (se houver contexto).</summary>
    public Guid? UserId { get; set; }
    /// <summary>Tipo de operação (Insert, Update, Delete).</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Nome da tabela afetada.</summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>Data e hora da alteração (UTC).</summary>
    public DateTime DateTime { get; set; }
    /// <summary>Valores das propriedades antes da alteração (JSON).</summary>
    public string OldValues { get; set; } = string.Empty;
    /// <summary>Valores das propriedades após a alteração (JSON).</summary>
    public string NewValues { get; set; } = string.Empty;
    /// <summary>Lista de colunas que foram modificadas.</summary>
    public string AffectedColumns { get; set; } = string.Empty;
    /// <summary>Valor da chave primária da entidade afetada (JSON).</summary>
    public string PrimaryKey { get; set; } = string.Empty;
}
