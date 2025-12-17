using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Data;

/// <summary>
/// Contrato de repositório para escrita e leitura de Aggregate Roots.
/// </summary>
/// <remarks>
/// Padrão Repository combinando persistência e unidade de trabalho.
/// - Gerencia o ciclo de vida completo do Aggregate Root
/// - Fornece acesso ao UnitOfWork para transações
/// - Abstrai o mecanismo de persistência (EF Core)
/// 
/// Exemplo de uso:
/// <code>
/// public class CriarPedidoHandler : ICommandHandler&lt;CriarPedidoCommand&gt;
/// {
///     private readonly IRepository&lt;Pedido&gt; _pedidoRepo;
///     
///     public async Task Handle(CriarPedidoCommand cmd, CancellationToken ct)
///     {
///         var pedido = new Pedido(cmd.ClienteId);
///         await _pedidoRepo.AddAsync(pedido, ct);
///         await _pedidoRepo.UnitOfWork.SaveChangesAsync(ct);
///     }
/// }
/// </code>
/// </remarks>
public interface IRepository<TEntity> where TEntity : class, IAggregateRoot
{
    /// <summary>Obtém a unidade de trabalho para controle transacional.</summary>
    IUnitOfWork UnitOfWork { get; }
    
    /// <summary>Obtém uma entidade pelo ID.</summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Adiciona uma nova entidade ao contexto.</summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>Marca a entidade como modificada no contexto.</summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>Marca a entidade para remoção física do banco.</summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
