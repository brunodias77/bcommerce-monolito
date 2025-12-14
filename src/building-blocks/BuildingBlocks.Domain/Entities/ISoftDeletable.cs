namespace BuildingBlocks.Domain.Entities;

/// <summary>
/// Interface para entidades que implementam Soft Delete.
/// </summary>
/// <remarks>
/// No seu schema PostgreSQL:
/// - deleted_at é nullable (TIMESTAMPTZ)
/// - Índices filtrados usam WHERE deleted_at IS NULL para performance
/// - Dados nunca são removidos fisicamente
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>
    /// Data e hora da exclusão lógica (UTC).
    /// Null indica que o registro está ativo.
    /// </summary>
    DateTime? DeletedAt { get; }

    /// <summary>
    /// Indica se o registro foi excluído logicamente.
    /// </summary>
    bool IsDeleted => DeletedAt.HasValue;

    /// <summary>
    /// Marca a entidade como excluída logicamente.
    /// </summary>
    void Delete();

    /// <summary>
    /// Restaura uma entidade excluída logicamente.
    /// </summary>
    void Restore();
}
