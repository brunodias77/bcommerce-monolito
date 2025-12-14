namespace BuildingBlocks.Domain.Repositories;

/// <summary>
/// Interface para Unit of Work pattern.
/// </summary>
/// <remarks>
/// No Entity Framework Core, o DbContext já implementa o padrão Unit of Work:
/// - ChangeTracker mantém o estado das entidades
/// - SaveChangesAsync persiste todas as mudanças em uma única transação
/// 
/// No seu sistema modular monolith:
/// - Cada módulo tem seu próprio DbContext
/// - Transações são locais ao módulo (não distribuídas entre módulos)
/// - Comunicação entre módulos via Eventos (Outbox Pattern)
/// 
/// Exemplo de implementação:
/// <code>
/// public class CatalogDbContext : DbContext, IUnitOfWork
/// {
///     public DbSet&lt;Product&gt; Products { get; set; }
///     public DbSet&lt;Category&gt; Categories { get; set; }
///     
///     public async Task&lt;int&gt; SaveChangesAsync(CancellationToken cancellationToken = default)
///     {
///         // Publicar eventos de domínio antes de salvar
///         await DispatchDomainEventsAsync(cancellationToken);
///         
///         // Salvar mudanças
///         return await base.SaveChangesAsync(cancellationToken);
///     }
///     
///     public async Task&lt;bool&gt; SaveEntitiesAsync(CancellationToken cancellationToken = default)
///     {
///         try
///         {
///             await SaveChangesAsync(cancellationToken);
///             return true;
///         }
///         catch
///         {
///             return false;
///         }
///     }
/// }
/// </code>
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Salva todas as mudanças pendentes no banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de registros afetados</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva todas as mudanças e retorna indicador de sucesso.
    /// Útil para validar se a operação foi bem-sucedida.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se salvou com sucesso, False caso contrário</returns>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}