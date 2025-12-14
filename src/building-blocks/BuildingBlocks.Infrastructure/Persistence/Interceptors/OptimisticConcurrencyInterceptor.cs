using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor para tratamento de erros de concorrência otimista.
/// </summary>
/// <remarks>
/// Este interceptor:
/// - Intercepta DbUpdateConcurrencyException
/// - Loga informações detalhadas sobre o conflito
/// - Pode ser configurado para retry automático (opcional)
/// 
/// No PostgreSQL, a concorrência otimista é implementada via:
/// - Coluna 'version' com trigger shared.trigger_increment_version()
/// - EF Core detecta conflitos comparando a versão
/// 
/// Configuração:
/// <code>
/// options.AddInterceptors(new OptimisticConcurrencyInterceptor(logger));
/// </code>
/// </remarks>
public class OptimisticConcurrencyInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<OptimisticConcurrencyInterceptor>? _logger;
    private readonly int _maxRetries;

    public OptimisticConcurrencyInterceptor(
        ILogger<OptimisticConcurrencyInterceptor>? logger = null,
        int maxRetries = 0)
    {
        _logger = logger;
        _maxRetries = maxRetries;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(
        DbContextErrorEventData eventData)
    {
        if (eventData.Exception is DbUpdateConcurrencyException concurrencyException)
        {
            HandleConcurrencyException(eventData.Context, concurrencyException);
        }

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Exception is DbUpdateConcurrencyException concurrencyException)
        {
            HandleConcurrencyException(eventData.Context, concurrencyException);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void HandleConcurrencyException(
        DbContext? context,
        DbUpdateConcurrencyException exception)
    {
        if (context is null)
            return;

        var entries = exception.Entries;

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Property("Id").CurrentValue;

            _logger?.LogWarning(
                "Concurrency conflict detected for {EntityType} with Id {EntityId}. " +
                "The entity was modified by another user or process.",
                entityType,
                entityId);

            // Log das propriedades em conflito
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();

            if (databaseValues != null)
            {
                foreach (var property in proposedValues.Properties)
                {
                    var proposedValue = proposedValues[property];
                    var databaseValue = databaseValues[property];

                    if (!Equals(proposedValue, databaseValue))
                    {
                        _logger?.LogDebug(
                            "Property {PropertyName}: Proposed={ProposedValue}, Database={DatabaseValue}",
                            property.Name,
                            proposedValue,
                            databaseValue);
                    }
                }
            }
        }
    }
}

/// <summary>
/// Interface para entidades com versionamento (concorrência otimista).
/// </summary>
public interface IVersionedEntity
{
    /// <summary>
    /// Versão da entidade para controle de concorrência.
    /// Incrementada automaticamente via trigger no PostgreSQL.
    /// </summary>
    int Version { get; }
}

/// <summary>
/// Extensões para configurar concorrência otimista no EF Core.
/// </summary>
public static class OptimisticConcurrencyExtensions
{
    /// <summary>
    /// Configura a propriedade Version como concurrency token.
    /// </summary>
    /// <remarks>
    /// Uso no EntityConfiguration:
    /// <code>
    /// builder.ConfigureOptimisticConcurrency();
    /// </code>
    /// </remarks>
    public static void ConfigureOptimisticConcurrency<TEntity>(
        this Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IVersionedEntity
    {
        builder.Property(e => e.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .HasDefaultValue(1);
    }
}
