using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que publica Domain Events no Outbox durante SaveChanges.
/// </summary>
/// <remarks>
/// Este é o CORE do Outbox Pattern no seu sistema!
///
/// Fluxo:
/// 1. Entidade levanta Domain Event: product.AddDomainEvent(new ProductCreatedEvent(...))
/// 2. SaveChangesAsync() é chamado
/// 3. Este interceptor:
///    a) Coleta todos os Domain Events das entidades
///    b) Obtém AggregateId da interface IDomainEvent
///    c) Obtém AggregateType do atributo [AggregateType] ou heurística
///    d) Serializa como JSON
///    e) Salva na tabela shared.domain_events (Outbox)
///    f) Limpa eventos da entidade
/// 4. Background Job processa o Outbox posteriormente
///
/// Schema PostgreSQL (shared.domain_events):
/// CREATE TABLE shared.domain_events (
///     id UUID PRIMARY KEY,
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
///
/// IMPORTANTE - Eventos de Domínio:
/// Todos os eventos devem:
/// 1. Herdar de DomainEvent
/// 2. Implementar a propriedade AggregateId
/// 3. (Recomendado) Usar o atributo [AggregateType("NomeDoAgregado")]
///
/// Exemplo:
/// <code>
/// [AggregateType("Product")]
/// public class ProductCreatedEvent : DomainEvent
/// {
///     public Guid ProductId { get; }
///     public override Guid AggregateId => ProductId;
///
///     public ProductCreatedEvent(Guid productId) => ProductId = productId;
/// }
/// </code>
/// </remarks>
public sealed class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly string _moduleName;
    private readonly ILogger<PublishDomainEventsInterceptor>? _logger;

    // Cache para atributos AggregateType (evita reflection repetida)
    private static readonly ConcurrentDictionary<Type, string> _aggregateTypeCache = new();

    /// <summary>
    /// Cria interceptor para um módulo específico.
    /// </summary>
    /// <param name="moduleName">Nome do módulo (users, catalog, orders, payments, coupons, cart)</param>
    /// <param name="logger">Logger opcional para warnings</param>
    public PublishDomainEventsInterceptor(string moduleName, ILogger<PublishDomainEventsInterceptor>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name is required", nameof(moduleName));

        _moduleName = moduleName.ToLowerInvariant();
        _logger = logger;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await PublishDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            PublishDomainEventsAsync(eventData.Context, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    private async Task PublishDomainEventsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        // Coleta todas as entidades com eventos de domínio
        var entitiesWithEvents = context.ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        if (!entitiesWithEvents.Any())
            return;

        // Coleta todos os eventos
        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        // Limpa eventos das entidades (importante para não reprocessar)
        entitiesWithEvents.ForEach(entity => entity.ClearDomainEvents());

        // Cria mensagens do Outbox
        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Module = _moduleName,
            AggregateType = GetAggregateTypeName(domainEvent),
            AggregateId = GetAggregateId(domainEvent),
            EventType = domainEvent.GetType().Name,
            Payload = SerializeEvent(domainEvent),
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null,
            ErrorMessage = null,
            RetryCount = 0
        }).ToList();

        // Salva no Outbox
        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
    }

    /// <summary>
    /// Obtém o nome do tipo do agregado usando o atributo [AggregateType] ou heurística.
    /// </summary>
    /// <remarks>
    /// Ordem de prioridade:
    /// 1. Atributo [AggregateType("Nome")] na classe do evento
    /// 2. Heurística: remove sufixo "Event" do nome da classe
    ///
    /// Resultados são cacheados para performance.
    /// </remarks>
    private string GetAggregateTypeName(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();

        return _aggregateTypeCache.GetOrAdd(eventType, type =>
        {
            // Tenta obter do atributo [AggregateType]
            var attribute = type.GetCustomAttribute<AggregateTypeAttribute>();
            if (attribute != null)
            {
                return attribute.Name;
            }

            // Fallback: heurística baseada no nome da classe
            var typeName = type.Name;

            // Remove "Event" do final se presente
            if (typeName.EndsWith("Event"))
                typeName = typeName[..^5];

            // Log warning para incentivar uso do atributo
            _logger?.LogWarning(
                "Event {EventType} does not have [AggregateType] attribute. " +
                "Using heuristic: '{AggregateType}'. Consider adding the attribute for explicit control.",
                type.Name,
                typeName);

            return typeName;
        });
    }

    /// <summary>
    /// Obtém o ID do agregado diretamente da interface IDomainEvent.
    /// </summary>
    /// <remarks>
    /// Usa a propriedade AggregateId definida na interface IDomainEvent.
    /// Isso é type-safe e evita reflection frágil.
    ///
    /// Se o evento não implementar IDomainEvent corretamente, retorna Guid.Empty
    /// e loga um warning.
    /// </remarks>
    private Guid GetAggregateId(IDomainEvent domainEvent)
    {
        var aggregateId = domainEvent.AggregateId;

        if (aggregateId == Guid.Empty)
        {
            _logger?.LogWarning(
                "Event {EventType} returned Guid.Empty for AggregateId. " +
                "Make sure the AggregateId property is implemented correctly.",
                domainEvent.GetType().Name);
        }

        return aggregateId;
    }

    private static string SerializeEvent(object domainEvent)
    {
        return JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
