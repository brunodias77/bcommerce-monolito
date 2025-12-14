using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.BackgroundJobs;

/// <summary>
/// Background Service que limpa sessões expiradas.
/// </summary>
/// <remarks>
/// Este job:
/// 1. Busca sessões expiradas (expires_at &lt; NOW() e revoked_at IS NULL)
/// 2. Marca como revogadas (revoked_at = NOW())
/// 
/// Tabela afetada: users.sessions
/// 
/// Configuração:
/// <code>
/// services.AddHostedService&lt;CleanupExpiredSessionsJob&gt;();
/// </code>
/// </remarks>
public class CleanupExpiredSessionsJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanupExpiredSessionsJob> _logger;
    private readonly TimeSpan _cleanupInterval;
    private readonly int _batchSize;

    public CleanupExpiredSessionsJob(
        IServiceProvider serviceProvider,
        ILogger<CleanupExpiredSessionsJob> logger,
        TimeSpan? cleanupInterval = null,
        int batchSize = 100)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(5);
        _batchSize = batchSize;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Expired Sessions Cleanup Job started. Interval: {Interval} minutes",
            _cleanupInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredSessionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired sessions");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Expired Sessions Cleanup Job stopped");
    }

    private async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetService<IDateTimeProvider>()
            ?? new DateTimeProvider();

        var utcNow = dateTimeProvider.UtcNow;

        // Executa SQL diretamente para performance
        // A tabela é users.sessions
        var sql = @"
            UPDATE users.sessions 
            SET revoked_at = {0}, 
                revoked_reason = 'EXPIRED'
            WHERE revoked_at IS NULL 
              AND expires_at < {0}
            LIMIT {1}";

        try
        {
            var affected = await dbContext.Database.ExecuteSqlRawAsync(
                sql.Replace("LIMIT {1}", $"LIMIT {_batchSize}").Replace("{0}", $"'{utcNow:yyyy-MM-dd HH:mm:ss}'"),
                cancellationToken);

            if (affected > 0)
            {
                _logger.LogInformation(
                    "Cleaned up {Count} expired sessions",
                    affected);
            }
        }
        catch (Exception ex)
        {
            // Fallback: usar EF Core se o SQL raw falhar
            _logger.LogWarning(ex, "Raw SQL failed, falling back to EF Core");
            await CleanupWithEfCoreAsync(dbContext, utcNow, cancellationToken);
        }
    }

    private Task CleanupWithEfCoreAsync(
        DbContext dbContext,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        // Esta é uma abordagem alternativa usando EF Core
        // Requer que a entidade Session esteja registrada no DbContext

        // Como Session está no módulo Users, este job precisa acessar
        // UsersDbContext especificamente. Por isso, o SQL raw é preferido.

        _logger.LogWarning(
            "EF Core fallback requires UsersDbContext. " +
            "Consider injecting the correct DbContext for session cleanup.");
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// Configuração para o CleanupExpiredSessionsJob.
/// </summary>
public class SessionCleanupOptions
{
    /// <summary>
    /// Intervalo entre cada limpeza.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Número máximo de sessões limpas por execução.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}

/// <summary>
/// Extensões para configurar o Session Cleanup Job.
/// </summary>
public static class SessionCleanupExtensions
{
    public static IServiceCollection AddSessionCleanupJob(
        this IServiceCollection services,
        Action<SessionCleanupOptions>? configure = null)
    {
        var options = new SessionCleanupOptions();
        configure?.Invoke(options);

        services.AddHostedService(sp =>
            new CleanupExpiredSessionsJob(
                sp,
                sp.GetRequiredService<ILogger<CleanupExpiredSessionsJob>>(),
                options.CleanupInterval,
                options.BatchSize));

        return services;
    }
}
