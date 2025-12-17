
using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics; // For Interceptors

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure.Data;

public class TestDbContext : BaseDbContext
{
    // Usually BaseDbContext handles IInterceptor injection via constructor if set up that way,
    // or we might need to pass them down.
    // Assuming standard constructor for now.
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestItem> TestItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContext).Assembly);
        modelBuilder.HasDefaultSchema("test_module");
    }
}
