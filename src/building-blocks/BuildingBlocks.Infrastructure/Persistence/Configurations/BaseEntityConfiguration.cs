using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração base para entidades que herdam de Entity.
/// </summary>
/// <remarks>
/// Aplica configurações padrão para:
/// - Id como chave primária
/// - Mapeamento de colunas snake_case
/// 
/// Uso:
/// <code>
/// public class ProductConfiguration : BaseEntityConfiguration&lt;Product&gt;
/// {
///     public override void Configure(EntityTypeBuilder&lt;Product&gt; builder)
///     {
///         base.Configure(builder);
///         
///         builder.ToTable("products", "catalog");
///         // ... outras configurações
///     }
/// }
/// </code>
/// </remarks>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Chave primária
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Guid gerado pela aplicação

        // Ignora DomainEvents (não persiste no banco)
        builder.Ignore(e => e.DomainEvents);
    }
}

/// <summary>
/// Configuração base para entidades auditáveis.
/// </summary>
public abstract class AuditableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
    where TEntity : Entity, IAuditableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}

/// <summary>
/// Configuração base para entidades com soft delete.
/// </summary>
public abstract class SoftDeletableEntityConfiguration<TEntity> : AuditableEntityConfiguration<TEntity>
    where TEntity : Entity, IAuditableEntity, ISoftDeletable
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at");

        // Ignora a propriedade computada IsDeleted
        builder.Ignore(e => e.IsDeleted);

        // Query filter global para excluir registros deletados
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
