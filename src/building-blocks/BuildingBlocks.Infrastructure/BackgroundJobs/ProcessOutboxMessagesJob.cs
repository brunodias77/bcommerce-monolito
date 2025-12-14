using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BuildingBlocks.Infrastructure.BackgroundJobs;

/// <summary>
/// Background Service que processa mensagens do Outbox.
/// </summary>
/// <remarks>
/// Este job:
/// 1. Busca mensagens não processadas do Outbox (shared.domain_events)
/// 2. Deserializa e publica para os handlers registrados
/// 3. Marca como processadas ou incrementa retry_count em caso de erro
/// 
/// Configuração:
/// <code>
/// services.AddOutboxProcessor&lt;UsersDbContext&gt;();
/// </code>
/// 
/// Configurações recomendadas:
/// - Intervalo de processamento: 1-5 segundos
/// - Batch size: 10-100 mensagens
/// - Max retries: 3-5
/// </remarks>
public class ProcessOutboxMessagesJob<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessOutboxMessagesJob<TDbContext>> _logger;
    private readonly TimeSpan _processInterval;
    private readonly int _batchSize;
    private readonly int _maxRetries;

    public ProcessOutboxMessagesJob(
        IServiceProvider serviceProvider,
        ILogger<ProcessOutboxMessagesJob<TDbContext>> logger,
        TimeSpan? processInterval = null,
        int batchSize = 20,
        int maxRetries = 3)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _processInterval = processInterval ?? TimeSpan.FromSeconds(2);
        _batchSize = batchSize;
        _maxRetries = maxRetries;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started for {DbContext}. Interval: {Interval}s, BatchSize: {BatchSize}",
            typeof(TDbContext).Name, _processInterval.TotalSeconds, _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        // Verifica se o DbContext tem a tabela de OutboxMessage mapeada
        try
        {
            // Busca mensagens não processadas
            var messages = await dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null && m.RetryCount < _maxRetries)
                .OrderBy(m => m.CreatedAt)
                .Take(_batchSize)
                .ToListAsync(cancellationToken);

            if (!messages.Any())
                return;

            _logger.LogDebug("Processing {Count} outbox messages", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    await ProcessMessageAsync(scope.ServiceProvider, message, cancellationToken);

                    message.ProcessedAt = DateTime.UtcNow;
                    message.ErrorMessage = null;

                    _logger.LogDebug("Processed outbox message {MessageId} of type {EventType}",
                        message.Id, message.EventType);
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.ErrorMessage = ex.Message;

                    _logger.LogWarning(ex,
                        "Error processing outbox message {MessageId}. Retry count: {RetryCount}",
                        message.Id, message.RetryCount);

                    if (message.RetryCount >= _maxRetries)
                    {
                        _logger.LogError(
                            "Max retries reached for outbox message {MessageId}. Moving to dead letter.",
                            message.Id);
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // OutboxMessage não está mapeada neste DbContext, ignorar silenciosamente
            _logger.LogDebug("{DbContext} does not have OutboxMessage mapped, skipping", typeof(TDbContext).Name);
        }
    }

    private async Task ProcessMessageAsync(
        IServiceProvider serviceProvider,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        // Deserializa o evento
        var eventType = Type.GetType(message.EventType);
        if (eventType == null)
        {
            throw new InvalidOperationException($"Event type not found: {message.EventType}");
        }

        var @event = JsonConvert.DeserializeObject(message.Payload, eventType, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        if (@event == null)
        {
            throw new InvalidOperationException($"Failed to deserialize event: {message.EventType}");
        }

        // Obtém os handlers registrados
        var handlers = Messaging.Integration.OutboxEventBus.GetHandlersForEvent(eventType);

        foreach (var handlerType in handlers)
        {
            var handler = serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                _logger.LogWarning("Handler {HandlerType} not found in DI container", handlerType.Name);
                continue;
            }

            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod != null)
            {
                var task = (Task?)handleMethod.Invoke(handler, new[] { @event, cancellationToken });
                if (task != null)
                {
                    await task;
                }
            }
        }
    }
}

/// <summary>
/// Configuração para o ProcessOutboxMessagesJob.
/// </summary>
public class OutboxProcessorOptions
{
    /// <summary>
    /// Intervalo entre cada processamento.
    /// </summary>
    public TimeSpan ProcessInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Número de mensagens processadas por vez.
    /// </summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// Número máximo de tentativas antes de mover para dead letter.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Extensões para configurar o Outbox Processor.
/// </summary>
public static class OutboxProcessorExtensions
{
    /// <summary>
    /// Adiciona o Outbox Processor para um DbContext específico.
    /// </summary>
    /// <typeparam name="TDbContext">DbContext que contém a tabela de OutboxMessage</typeparam>
    public static IServiceCollection AddOutboxProcessor<TDbContext>(
        this IServiceCollection services,
        Action<OutboxProcessorOptions>? configure = null)
        where TDbContext : DbContext
    {
        var options = new OutboxProcessorOptions();
        configure?.Invoke(options);

        services.AddHostedService(sp =>
            new ProcessOutboxMessagesJob<TDbContext>(
                sp,
                sp.GetRequiredService<ILogger<ProcessOutboxMessagesJob<TDbContext>>>(),
                options.ProcessInterval,
                options.BatchSize,
                options.MaxRetries));

        return services;
    }
}
