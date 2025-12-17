using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração base do EF Core para Agregados.
/// </summary>
/// <remarks>
/// Especialização de EntityConfiguration para AggregateRoots.
/// - Ignora a coleção de DomainEvents (não persistida)
/// - Garante configurações padrão de Entidade
/// 
/// Exemplo de uso:
/// <code>
/// public class ProductConfiguration : AggregateRootConfiguration&lt;Product&gt; { ... }
/// </code>
/// </remarks>
public abstract class AggregateRootConfiguration<TEntity> : EntityConfiguration<TEntity>
    where TEntity : class, IAggregateRoot
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);
        
        // IAggregateRoot geralmente não tem campos de banco extras além de IEntity
        // mas é um bom lugar para ignorar DomainEvents
        
        builder.Ignore(e => e.DomainEvents);
    }
}
