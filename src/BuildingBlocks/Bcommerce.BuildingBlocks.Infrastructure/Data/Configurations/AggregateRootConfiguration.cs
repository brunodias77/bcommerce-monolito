using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;

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
