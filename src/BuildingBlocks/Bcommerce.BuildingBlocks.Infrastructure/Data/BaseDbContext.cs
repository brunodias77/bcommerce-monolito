using System.Reflection;
using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data;

/// <summary>
/// Contexto base do Entity Framework Core para a aplicação.
/// </summary>
/// <remarks>
/// Configura a conexão e interceptadores do EF Core.
/// - Aplica interceptors para Auditoria, SoftDelete e Outbox
/// - Carrega configurações de mapeamento via Reflection
/// - Serve como base para contextos específicos de módulos
/// 
/// Exemplo de uso:
/// <code>
/// public class CatalogDbContext(DbContextOptions opts, ...) : BaseDbContext(opts, ...)
/// {
///     public DbSet&lt;Product&gt; Products { get; set; }
/// }
/// </code>
/// </remarks>
public abstract class BaseDbContext(
    DbContextOptions options,
    AuditableEntityInterceptor auditableEntityInterceptor,
    SoftDeleteInterceptor softDeleteInterceptor,
    DomainEventInterceptor domainEventInterceptor, // Opcional se usar apenas Outbox
    OptimisticLockInterceptor optimisticLockInterceptor)
    : DbContext(options), IUnitOfWork
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor = auditableEntityInterceptor;
    private readonly SoftDeleteInterceptor _softDeleteInterceptor = softDeleteInterceptor;
    private readonly DomainEventInterceptor _domainEventInterceptor = domainEventInterceptor;
    private readonly OptimisticLockInterceptor _optimisticLockInterceptor = optimisticLockInterceptor;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(
            _auditableEntityInterceptor, 
            _softDeleteInterceptor, 
            // _domainEventInterceptor, // Descomentar para disparar eventos síncronos sem Outbox
            _optimisticLockInterceptor);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDbContext).Assembly);
        
        // Aplica configurações globais, se necessário (ex: snake_case naming convention)
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
