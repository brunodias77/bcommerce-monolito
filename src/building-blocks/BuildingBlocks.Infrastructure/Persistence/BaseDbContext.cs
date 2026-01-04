using BuildingBlocks.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Classe base para DbContext com configurações comuns
/// </summary>
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Obtém todas as entidades do tipo especificado que possuem eventos de domínio pendentes
    /// </summary>
    /// <returns>Lista de entidades com eventos de domínio</returns>
    public IEnumerable<AggregateRoot> GetEntitiesWithDomainEvents()
    {
        return ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();
    }

    /// <summary>
    /// Limpa os eventos de domínio de todas as entidades rastreadas
    /// </summary>
    public void ClearDomainEvents()
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configura convenções globais
        ConfigureGlobalConventions(modelBuilder);

        // Ignora a propriedade DomainEvents de todas as entidades
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AggregateRoot).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore(nameof(AggregateRoot.DomainEvents));
            }
        }
    }

    /// <summary>
    /// Configura convenções globais para todas as entidades
    /// </summary>
    private void ConfigureGlobalConventions(ModelBuilder modelBuilder)
    {
        // Configuração global para strings
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(string)))
        {
            // Define tamanho máximo padrão para strings sem tamanho especificado
            if (property.GetMaxLength() == null)
            {
                property.SetMaxLength(256);
            }
        }

        // Configuração global para decimais (precisão para valores monetários)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            if (property.GetPrecision() == null)
            {
                property.SetPrecision(10);
                property.SetScale(2);
            }
        }

        // Configuração global para timestamps
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configura índices para soft delete
            if (entityType.FindProperty("DeletedAt") != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("DeletedAt")
                    .HasFilter("deleted_at IS NULL");
            }

            // Configura índices para campos de auditoria
            if (entityType.FindProperty("CreatedAt") != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .HasDefaultValueSql("NOW()");
            }

            if (entityType.FindProperty("UpdatedAt") != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("UpdatedAt")
                    .HasDefaultValueSql("NOW()");
            }
        }
    }
}
