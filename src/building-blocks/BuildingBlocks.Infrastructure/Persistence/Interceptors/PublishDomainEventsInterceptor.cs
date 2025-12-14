using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que salva domain events no Outbox durante SaveChanges.
/// </summary>
/// <remarks>
/// Este interceptor implementa o Outbox Pattern:
/// 1. Coleta todos os domain events das entidades modificadas
/// 2. Serializa e salva os eventos na tabela shared.domain_events
/// 3. Limpa os eventos das entidades
/// 
/// Os eventos são processados posteriormente pelo ProcessOutboxMessagesJob.
/// 
/// Configuração:
/// <code>
/// options.AddInterceptors(new PublishDomainEventsInterceptor("users"));
/// </code>
/// </remarks>
public class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Nome do módulo (users, catalog, orders, etc.).
    /// </summary>
    public string ModuleName { get; }

    public PublishDomainEventsInterceptor(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name is required", nameof(moduleName));

        ModuleName = moduleName;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await SaveDomainEventsToOutboxAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SaveDomainEventsToOutboxAsync(eventData.Context, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        return base.SavingChanges(eventData, result);
    }

    private async Task SaveDomainEventsToOutboxAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
            return;

        // Coleta todas as entidades que possuem domain events
        var entitiesWithEvents = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        if (!entitiesWithEvents.Any())
            return;

        // Coleta todos os eventos
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents.Select(ev => new { Entity = e, Event = ev }))
            .ToList();

        // Limpa os eventos das entidades (antes de salvar para evitar duplicação)
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Salva os eventos no Outbox
        foreach (var item in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = item.Event.EventId,
                Module = ModuleName,
                AggregateType = GetAggregateType(item.Event),
                AggregateId = item.Event.AggregateId,
                EventType = item.Event.GetType().FullName ?? item.Event.GetType().Name,
                Payload = SerializeEvent(item.Event),
                CreatedAt = item.Event.OccurredOn
            };

            await context.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
        }
    }

    private static string GetAggregateType(IDomainEvent domainEvent)
    {
        // Tenta obter do atributo AggregateType
        var attribute = domainEvent.GetType()
            .GetCustomAttributes(typeof(AggregateTypeAttribute), false)
            .FirstOrDefault() as AggregateTypeAttribute;

        if (attribute != null)
            return attribute.Name;

        // Fallback: remove "Event" do nome da classe e tenta extrair o nome do agregado
        var typeName = domainEvent.GetType().Name;
        if (typeName.EndsWith("Event"))
            typeName = typeName[..^5]; // Remove "Event"

        // Tenta extrair o nome do agregado (ex: "ProductCreated" -> "Product")
        var commonSuffixes = new[] { "Created", "Updated", "Deleted", "Changed", "Added", "Removed" };
        foreach (var suffix in commonSuffixes)
        {
            if (typeName.EndsWith(suffix))
                return typeName[..^suffix.Length];
        }

        return typeName;
    }

    private static string SerializeEvent(IDomainEvent domainEvent)
    {
        return JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    }
}

/// <summary>
/// Entidade para a tabela shared.domain_events (Outbox).
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Module { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
