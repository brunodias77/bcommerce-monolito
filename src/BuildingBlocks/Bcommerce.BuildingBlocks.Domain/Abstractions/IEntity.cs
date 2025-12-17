namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Contrato base para todas as entidades de domínio.
/// </summary>
/// <remarks>
/// Representa objetos com identidade contínua no domínio.
/// - Possui identidade única que persiste ao longo do tempo
/// - Rastreamento de datas de criação/atualização
/// - Base para Entity&lt;TId&gt; e AggregateRoot&lt;TId&gt;
/// 
/// Exemplo de uso:
/// <code>
/// public class ItemPedido : Entity&lt;Guid&gt;, IEntity
/// {
///     public ProdutoId ProdutoId { get; }
///     public int Quantidade { get; }
/// }
/// </code>
/// </remarks>
public interface IEntity
{
    /// <summary>Data de criação da entidade (UTC).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Data da última atualização (UTC), null se nunca atualizado.</summary>
    public DateTime? UpdatedAt { get; set; }
}
