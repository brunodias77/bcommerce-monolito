using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração base do EF Core para Entidades de Domínio.
/// </summary>
/// <remarks>
/// Define mapeamentos comuns para todas as entidades do sistema.
/// - Configura campos de auditoria (CreatedAt, UpdatedAt)
/// - Aplica filtros globais de Soft Delete (IsDeleted)
/// - Configura controle de concorrência (Version)
/// 
/// Exemplo de uso:
/// <code>
/// public class OrderItemConfiguration : EntityConfiguration&lt;OrderItem&gt; { ... }
/// </code>
/// </remarks>
public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Configurações padrão para todas IEntity
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        
        // Se ISoftDeletable
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(nameof(ISoftDeletable.IsDeleted)).IsRequired().HasDefaultValue(false);
            builder.Property(nameof(ISoftDeletable.DeletedAt));
            builder.HasQueryFilter(e => !((ISoftDeletable)e).IsDeleted);
        }

        // Se IVersionable
        if (typeof(IVersionable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(nameof(IVersionable.Version)).IsConcurrencyToken();
        }
    }
}
