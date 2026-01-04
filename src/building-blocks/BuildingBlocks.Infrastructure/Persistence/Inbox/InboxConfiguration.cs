using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Persistence.Inbox;

/// <summary>
/// Configuração do mapeamento EF Core para a tabela shared.processed_events
/// </summary>
public sealed class InboxConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        // Nome da tabela e schema
        builder.ToTable("processed_events", "shared");

        // Chave primária
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Gerado pela aplicação (usa o ID do evento original)

        // Propriedades
        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Module)
            .HasColumnName("module")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Índice para buscas rápidas por ID (chave primária já cria um índice)
        // Sem índices adicionais necessários conforme schema.sql
    }
}