using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Representa uma mensagem de evento no Outbox.
/// Mapeia para a tabela shared.domain_events do PostgreSQL.
/// </summary>
/// <remarks>
/// Schema PostgreSQL:
/// <code>
/// CREATE TABLE shared.domain_events (
///     id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
///     module VARCHAR(50) NOT NULL,
///     aggregate_type VARCHAR(100) NOT NULL,
///     aggregate_id UUID NOT NULL,
///     event_type VARCHAR(100) NOT NULL,
///     payload JSONB NOT NULL,
///     created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
///     processed_at TIMESTAMPTZ,
///     error_message TEXT,
///     retry_count INT DEFAULT 0
/// );
/// </code>
/// 
/// Fluxo do Outbox Pattern:
/// 1. Domain Event é levantado na entidade
/// 2. PublishDomainEventsInterceptor salva no Outbox (mesma transação)
/// 3. Background Job (OutboxProcessor) processa periodicamente
/// 4. Evento é publicado via MediatR
/// 5. processed_at é definido
/// 
/// Se falhar:
/// - error_message é preenchido
/// - retry_count é incrementado
/// - Background job tenta novamente
/// </remarks>
public class OutboxMessage
{
    /// <summary>
    /// Identificador único da mensagem.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do módulo que originou o evento.
    /// Valores: "users", "catalog", "cart", "orders", "payments", "coupons"
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do agregado (Product, Order, Payment, etc.)
    /// </summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>
    /// ID do agregado que levantou o evento.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// Tipo do evento (ProductCreatedEvent, OrderPaidEvent, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Payload do evento serializado como JSON.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em que o evento foi criado (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data e hora em que o evento foi processado (UTC).
    /// Null = ainda não processado.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Mensagem de erro se o processamento falhou.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Número de tentativas de processamento.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Indica se a mensagem foi processada com sucesso.
    /// </summary>
    public bool IsProcessed => ProcessedAt.HasValue;

    /// <summary>
    /// Indica se a mensagem falhou após múltiplas tentativas.
    /// </summary>
    public bool IsFailed => !IsProcessed && RetryCount >= 3;
}

/// <summary>
/// Configuração EF Core para OutboxMessage.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("domain_events", "shared");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.Module)
            .HasColumnName("module")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(e => e.AggregateType)
            .HasColumnName("aggregate_type")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(e => e.AggregateId)
            .HasColumnName("aggregate_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(e => e.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz");

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasColumnType("text");

        builder.Property(e => e.RetryCount)
            .HasColumnName("retry_count")
            .HasColumnType("integer")
            .HasDefaultValue(0)
            .IsRequired();

        // Índices para performance
        builder.HasIndex(e => new { e.Module, e.AggregateType, e.AggregateId })
            .HasDatabaseName("idx_domain_events_module");

        builder.HasIndex(e => e.CreatedAt)
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("idx_domain_events_unprocessed");
    }
}