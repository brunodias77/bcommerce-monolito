using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Configuration;

/// <summary>
/// Configuração do Entity Framework para a tabela de Outbox Messages.
/// </summary>
/// <remarks>
/// Define o esquema da tabela OutboxMessages.
/// - Mapeia chave primária e campos obrigatórios
/// - Define limites de tamanho para otimização
/// - Usada pelo DbContext para criação da tabela
/// 
/// Exemplo de uso:
/// <code>
/// modelBuilder.ApplyConfiguration(new OutboxConfiguration());
/// </code>
/// </remarks>
public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Type).IsRequired();
    }
}
