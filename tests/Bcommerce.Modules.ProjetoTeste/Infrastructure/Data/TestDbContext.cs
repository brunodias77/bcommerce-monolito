
using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure.Data;

public class TestDbContext : BaseDbContext
{
    public TestDbContext(
        DbContextOptions<TestDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor,
        SoftDeleteInterceptor softDeleteInterceptor,
        DomainEventInterceptor domainEventInterceptor,
        OptimisticLockInterceptor optimisticLockInterceptor)
        : base(options, auditableEntityInterceptor, softDeleteInterceptor, domainEventInterceptor, optimisticLockInterceptor)
    {
    }

    public DbSet<TestItem> TestItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContext).Assembly);
        modelBuilder.HasDefaultSchema("test_module");
    }
}
