using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Messaging.Integration;

/// <summary>
/// Implementação do EventBus que salva Integration Events no Outbox.
/// </summary>
public class EventBus : IEventBus
{
    private readonly DbContext _dbContext;
    private readonly string _moduleName;

    public EventBus(DbContext dbContext, string moduleName)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _moduleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var outboxMessage = CreateOutboxMessage(@event);

        await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
    }

    public async Task PublishAsync(
        IEnumerable<IIntegrationEvent> events,
        CancellationToken cancellationToken = default)
    {
        var outboxMessages = events.Select(CreateOutboxMessage).ToList();

        await _dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
    }

    private OutboxMessage CreateOutboxMessage(IIntegrationEvent integrationEvent)
    {
        return new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Module = _moduleName,
            AggregateType = integrationEvent.GetType().Name.Replace("IntegrationEvent", ""),
            AggregateId = ExtractAggregateId(integrationEvent),
            EventType = integrationEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            CreatedAt = integrationEvent.OccurredOn,
            ProcessedAt = null,
            ErrorMessage = null,
            RetryCount = 0
        };
    }

    private static Guid ExtractAggregateId(IIntegrationEvent integrationEvent)
    {
        // Tenta extrair o ID do agregado via reflection
        var idProperty = integrationEvent.GetType()
            .GetProperties()
            .FirstOrDefault(p =>
                p.Name.EndsWith("Id") &&
                p.PropertyType == typeof(Guid));

        if (idProperty != null)
        {
            var value = idProperty.GetValue(integrationEvent);
            if (value is Guid id)
                return id;
        }

        return Guid.Empty;
    }
}

/// <summary>
/// Extensões para facilitar registro do EventBus.
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Registra o EventBus para um módulo específico.
    /// </summary>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        string moduleName)
    {
        services.AddScoped<IEventBus>(provider =>
        {
            var dbContext = provider.GetRequiredService<DbContext>();
            return new EventBus(dbContext, moduleName);
        });

        return services;
    }
}