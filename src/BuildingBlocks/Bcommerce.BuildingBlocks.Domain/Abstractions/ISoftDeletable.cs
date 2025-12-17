namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Contrato para entidades que suportam exclusão lógica (soft delete).
/// </summary>
/// <remarks>
/// Permite "excluir" entidades sem removê-las fisicamente do banco.
/// - Preserva histórico e integridade referencial
/// - Permite auditoria e recuperação
/// - Filtrado automaticamente em queries (configurar no DbContext)
/// 
/// Exemplo de uso:
/// <code>
/// public class Produto : AggregateRoot&lt;Guid&gt;, ISoftDeletable
/// {
///     public bool IsDeleted { get; set; }
///     public DateTime? DeletedAt { get; set; }
///     
///     public void Excluir()
///     {
///         IsDeleted = true;
///         DeletedAt = DateTime.UtcNow;
///     }
///     
///     public void UndoDelete()
///     {
///         IsDeleted = false;
///         DeletedAt = null;
///     }
/// }
/// </code>
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>Indica se a entidade foi excluída logicamente.</summary>
    public bool IsDeleted { get; set; }
    /// <summary>Data/hora da exclusão lógica (UTC).</summary>
    public DateTime? DeletedAt { get; set; }
    /// <summary>Reverte a exclusão lógica.</summary>
    public void UndoDelete();
}
