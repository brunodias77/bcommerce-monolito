using BuildingBlocks.Domain.Repositories;
using Cart.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Persistence;

/// <summary>
/// DbContext para o módulo Cart.
/// Corresponde ao schema 'cart' no banco de dados.
/// </summary>
public class CartDbContext : DbContext, IUnitOfWork
{
    public const string Schema = "cart";

    public DbSet<Core.Entities.Cart> Carts => Set<Core.Entities.Cart>();
    public DbSet<CartItem> Items => Set<CartItem>();

    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        // Aplica todas as configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
    }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

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
}
