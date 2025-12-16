using System.Reflection;
using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data;

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
