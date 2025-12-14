using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração base para todas as entidades do domínio.
/// Mapeia propriedades comuns: Id, CreatedAt, UpdatedAt, DeletedAt, Version.
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade</typeparam>
/// <remarks>
/// Esta classe base configura:
/// - Id como chave primária (UUID)
/// - CreatedAt e UpdatedAt (gerenciados por interceptor)
/// - DeletedAt para soft delete (gerenciado por interceptor)
/// - Version para optimistic concurrency (gerenciado por interceptor)
/// - Ignora DomainEvents (não persistido, apenas em memória)
/// 
/// No PostgreSQL, os triggers do schema fazem parte do trabalho,
/// mas os interceptors do EF Core garantem que tudo funcione corretamente
/// mesmo sem os triggers (útil para testes).
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
            .HasColumnType("uuid")
            .IsRequired();

        // Ignora DomainEvents (não deve ser persistido)
        builder.Ignore(e => e.DomainEvents);

        // Se é AggregateRoot, configura Version
        if (typeof(AggregateRoot).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property("Version")
                .HasColumnName("version")
                .HasColumnType("integer")
                .HasDefaultValue(1)
                .IsConcurrencyToken(); // Optimistic Concurrency Control
        }

        // Se implementa IAuditableEntity, configura timestamps
        if (typeof(IAuditableEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(nameof(IAuditableEntity.CreatedAt))
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(nameof(IAuditableEntity.UpdatedAt))
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .IsRequired();
        }

        // Se implementa ISoftDeletable, configura DeletedAt
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(nameof(ISoftDeletable.DeletedAt))
                .HasColumnName("deleted_at")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            // Query Filter global: não retorna registros deletados
            builder.HasQueryFilter(e => EF.Property<DateTime?>(e, "DeletedAt") == null);
        }

        // Configurações adicionais específicas da entidade
        ConfigureEntity(builder);
    }

    /// <summary>
    /// Sobrescreva este método para adicionar configurações específicas da entidade.
    /// </summary>
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}