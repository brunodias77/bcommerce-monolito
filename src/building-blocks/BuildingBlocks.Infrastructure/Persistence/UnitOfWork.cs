using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Implementação base do IUnitOfWork que encapsula o DbContext.
/// </summary>
/// <remarks>
/// Esta classe pode ser usada de duas formas:
/// 
/// 1. Herança direta (quando o DbContext não precisa herdar de outro):
/// <code>
/// public class CatalogDbContext : UnitOfWork
/// {
///     public DbSet&lt;Product&gt; Products { get; set; }
/// }
/// </code>
/// 
/// 2. Composição (quando o DbContext já herda de outro, ex: IdentityDbContext):
/// <code>
/// public class UsersDbContext : IdentityDbContext, IUnitOfWork
/// {
///     // Implementar IUnitOfWork diretamente
/// }
/// </code>
/// 
/// Para o caso 2, use as extensões em UnitOfWorkExtensions.
/// </remarks>
public class UnitOfWork : DbContext, IUnitOfWork
{
    public UnitOfWork(DbContextOptions options) : base(options)
    {
    }

    protected UnitOfWork() : base()
    {
    }

    /// <summary>
    /// Salva todas as mudanças pendentes.
    /// </summary>
    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Salva todas as mudanças e retorna indicador de sucesso.
    /// </summary>
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Inicia uma nova transação.
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se há uma transação ativa.
    /// </summary>
    public bool HasActiveTransaction => Database.CurrentTransaction != null;

    /// <summary>
    /// Obtém a transação atual.
    /// </summary>
    public IDbContextTransaction? CurrentTransaction => Database.CurrentTransaction;
}
