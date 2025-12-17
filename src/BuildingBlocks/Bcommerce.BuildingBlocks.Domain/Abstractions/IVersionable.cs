namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Contrato para entidades com controle de versão (concorrência otimista).
/// </summary>
/// <remarks>
/// Permite detectar e resolver conflitos de atualização concorrente.
/// - Incrementado a cada atualização
/// - Usado como RowVersion/ConcurrencyToken no EF Core
/// - Evita sobrescrever alterações de outros usuários
/// 
/// Exemplo de uso:
/// <code>
/// // No DbContext:
/// modelBuilder.Entity&lt;Produto&gt;()
///     .Property(p => p.Version)
///     .IsConcurrencyToken();
/// 
/// // Na entidade:
/// public class Produto : AggregateRoot&lt;Guid&gt;, IVersionable
/// {
///     public int Version { get; set; }
/// }
/// </code>
/// </remarks>
public interface IVersionable
{
    /// <summary>Número da versão atual da entidade.</summary>
    public int Version { get; set; }
}
