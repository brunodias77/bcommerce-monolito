namespace BuildingBlocks.Domain.Entities;

/// <summary>
/// Interface para entidades que possuem auditoria de timestamps.
/// </summary>
/// <remarks>
/// No seu schema PostgreSQL:
/// - created_at: Definido como DEFAULT NOW()
/// - updated_at: Atualizado via trigger shared.trigger_set_timestamp()
/// </remarks>
public interface IAuditableEntity
{
    /// <summary>
    /// Data e hora de criação da entidade (UTC).
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Data e hora da última atualização (UTC).
    /// Atualizada automaticamente via trigger no PostgreSQL.
    /// </summary>
    DateTime UpdatedAt { get; }
}