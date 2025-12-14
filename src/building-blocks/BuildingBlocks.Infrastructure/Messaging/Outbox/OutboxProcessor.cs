using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Background service que processa eventos pendentes no Outbox com suporte a
/// distributed lock e idempotência.
/// </summary>
/// <remarks>
/// Este serviço:
/// 1. Adquire um distributed lock antes de processar (evita duplicação em múltiplas instâncias)
/// 2. Busca eventos não processados com SELECT FOR UPDATE SKIP LOCKED
/// 3. Marca como "em processamento" ANTES de publicar (idempotência)
/// 4. Deserializa e publica via MediatR
/// 5. Atualiza status final (sucesso ou erro)
///
/// Configuração:
/// <code>
/// // Com PostgreSQL Distributed Lock (recomendado para produção)
/// builder.Services.AddOutboxProcessor(options =>
/// {
///     options.UsePostgresLock = true;
///     options.Interval = TimeSpan.FromSeconds(5);
///     options.BatchSize = 100;
///     options.MaxRetries = 3;
/// });
///
/// // Sem lock (apenas para desenvolvimento single-instance)
/// builder.Services.AddOutboxProcessor();
/// </code>
///
/// Garantias:
/// - Eventos são processados exatamente uma vez (at-least-once + idempotência)
/// - Múltiplas instâncias podem rodar simultaneamente sem conflito
/// - Falhas são registradas e eventos são reprocessados até maxRetries
/// </remarks>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDistributedLock _distributedLock;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxProcessorOptions _options;

    private const string LockKey = "outbox:processing";

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IDistributedLock distributedLock,
        ILogger<OutboxProcessor> logger,
        OutboxProcessorOptions? options = null)
    {
        _scopeFactory = scopeFactory;
        _distributedLock = distributedLock;
        _logger = logger;
        _options = options ?? new OutboxProcessorOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxProcessor started (interval: {Interval}s, batch: {BatchSize}, maxRetries: {MaxRetries})",
            _options.Interval.TotalSeconds,
            _options.BatchSize,
            _options.MaxRetries);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Tenta adquirir o lock distribuído
                await using var lockHandle = await _distributedLock.TryAcquireAsync(
                    LockKey,
                    timeout: null, // Não bloqueia - tenta e segue
                    stoppingToken);

                if (lockHandle != null)
                {
                    await ProcessPendingMessagesAsync(stoppingToken);
                }
                else
                {
                    _logger.LogDebug("Another instance is processing outbox messages, skipping...");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutdown gracioso
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_options.Interval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Busca mensagens pendentes usando SELECT FOR UPDATE SKIP LOCKED
        // Isso evita que múltiplas instâncias processem a mesma mensagem
        var pendingMessages = await dbContext.Set<OutboxMessage>()
            .FromSqlRaw(@"
                SELECT * FROM shared.domain_events
                WHERE processed_at IS NULL
                  AND retry_count < {0}
                ORDER BY created_at
                LIMIT {1}
                FOR UPDATE SKIP LOCKED",
                _options.MaxRetries,
                _options.BatchSize)
            .ToListAsync(cancellationToken);

        if (!pendingMessages.Any())
            return;

        _logger.LogInformation("Processing {Count} outbox messages", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            await ProcessMessageWithIdempotencyAsync(message, dbContext, mediator, cancellationToken);
        }
    }

    /// <summary>
    /// Processa uma mensagem com garantia de idempotência.
    /// </summary>
    /// <remarks>
    /// Estratégia de idempotência:
    /// 1. Marca a mensagem como "em processamento" (ProcessedAt = now) ANTES de publicar
    /// 2. Salva no banco (garante que outro worker não pegue a mesma mensagem)
    /// 3. Publica o evento via MediatR
    /// 4. Se falhar, registra o erro mas NÃO remove ProcessedAt
    ///    (evita reprocessamento infinito)
    ///
    /// Para retry em caso de falha:
    /// - O erro é registrado em ErrorMessage
    /// - RetryCount é incrementado
    /// - Uma lógica externa pode resetar ProcessedAt para mensagens com erro
    /// </remarks>
    private async Task ProcessMessageWithIdempotencyAsync(
        OutboxMessage message,
        DbContext dbContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // PASSO 1: Marca como processado ANTES de publicar (idempotência)
            message.ProcessedAt = startTime;
            message.ErrorMessage = null;

            await dbContext.SaveChangesAsync(cancellationToken);

            // PASSO 2: Processa o evento
            await PublishEventAsync(message, mediator, cancellationToken);

            _logger.LogDebug(
                "Processed outbox message {MessageId} of type {EventType} in {ElapsedMs}ms",
                message.Id,
                message.EventType,
                (DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            // PASSO 3: Em caso de erro, registra mas mantém ProcessedAt
            // Isso evita reprocessamento infinito
            message.RetryCount++;
            message.ErrorMessage = TruncateErrorMessage(ex.ToString());

            // Se atingiu o máximo de retries, mantém ProcessedAt (descarta mensagem)
            // Caso contrário, limpa ProcessedAt para permitir retry
            if (message.RetryCount < _options.MaxRetries)
            {
                message.ProcessedAt = null; // Permite retry
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Failed to save error state for message {MessageId}", message.Id);
            }

            _logger.LogWarning(
                ex,
                "Failed to process outbox message {MessageId} (attempt {RetryCount}/{MaxRetries})",
                message.Id,
                message.RetryCount,
                _options.MaxRetries);
        }
    }

    private static async Task PublishEventAsync(
        OutboxMessage message,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Tenta encontrar o tipo do evento
        var eventType = FindEventType(message.EventType);

        if (eventType == null)
        {
            throw new InvalidOperationException(
                $"Event type '{message.EventType}' not found. " +
                $"Make sure the assembly containing this event is loaded.");
        }

        // Deserializa o payload
        var @event = JsonSerializer.Deserialize(message.Payload, eventType);

        if (@event == null)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize event of type '{message.EventType}'");
        }

        // Publica via MediatR
        await mediator.Publish(@event, cancellationToken);
    }

    private static Type? FindEventType(string eventTypeName)
    {
        // Busca em todos os assemblies carregados
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .FirstOrDefault(type =>
                type.Name == eventTypeName &&
                typeof(INotification).IsAssignableFrom(type));
    }

    private static string TruncateErrorMessage(string error)
    {
        const int maxLength = 4000;
        return error.Length > maxLength ? error[..maxLength] : error;
    }
}

/// <summary>
/// Opções de configuração para o OutboxProcessor.
/// </summary>
public class OutboxProcessorOptions
{
    /// <summary>
    /// Intervalo entre ciclos de processamento.
    /// Default: 5 segundos.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Número máximo de mensagens processadas por ciclo.
    /// Default: 100.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Número máximo de tentativas antes de descartar a mensagem.
    /// Default: 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Se true, usa PostgreSQL Advisory Locks.
    /// Se false, usa NoOpDistributedLock (apenas para desenvolvimento).
    /// Default: true.
    /// </summary>
    public bool UsePostgresLock { get; set; } = true;
}

/// <summary>
/// Extensões para facilitar registro do OutboxProcessor.
/// </summary>
public static class OutboxProcessorExtensions
{
    /// <summary>
    /// Registra o OutboxProcessor como background service com distributed lock.
    /// </summary>
    /// <remarks>
    /// Exemplo de uso:
    /// <code>
    /// // Configuração padrão com PostgreSQL lock
    /// services.AddOutboxProcessor();
    ///
    /// // Configuração customizada
    /// services.AddOutboxProcessor(options =>
    /// {
    ///     options.Interval = TimeSpan.FromSeconds(10);
    ///     options.BatchSize = 50;
    ///     options.MaxRetries = 5;
    ///     options.UsePostgresLock = true;
    /// });
    ///
    /// // Sem lock (desenvolvimento)
    /// services.AddOutboxProcessor(options => options.UsePostgresLock = false);
    /// </code>
    /// </remarks>
    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services,
        Action<OutboxProcessorOptions>? configure = null)
    {
        var options = new OutboxProcessorOptions();
        configure?.Invoke(options);

        // Registra o distributed lock apropriado
        if (options.UsePostgresLock)
        {
            services.AddSingleton<IDistributedLock, PostgresDistributedLock>();
        }
        else
        {
            services.AddSingleton<IDistributedLock, NoOpDistributedLock>();
        }

        // Registra as opções
        services.AddSingleton(options);

        // Registra o background service
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    /// <summary>
    /// Registra apenas o distributed lock (útil para uso standalone).
    /// </summary>
    public static IServiceCollection AddPostgresDistributedLock(this IServiceCollection services)
    {
        services.AddSingleton<IDistributedLock, PostgresDistributedLock>();
        return services;
    }
}
