using BuildingBlocks.Application.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Behavior do MediatR para logging automático de requests e responses.
/// Registra entrada, saída, duração e erros.
/// </summary>
/// <remarks>
/// Este behavior intercepta todos os requests e:
/// 1. Loga informações do request (tipo, dados)
/// 2. Mede tempo de execução
/// 3. Loga resultado (sucesso/falha)
/// 4. Loga exceções se ocorrerem
/// 
/// Exemplo de logs gerados:
/// <code>
/// [INFO] Handling CreateProductCommand { Sku: "PROD-001", Name: "Product 1" }
/// [INFO] Handled CreateProductCommand in 234ms with result: Success
/// [ERROR] Handled CreateProductCommand in 1234ms with result: Failure - VALIDATION_ERROR
/// </code>
/// 
/// Configuração:
/// <code>
/// services.AddScoped(typeof(IPipelineBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;));
/// </code>
/// </remarks>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Log início da execução
            _logger.LogInformation(
                "Handling {RequestName} {@Request}",
                requestName,
                request);

            // Executa o handler
            var response = await next();

            stopwatch.Stop();

            // Log resultado
            if (response.IsSuccess)
            {
                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms with result: Success",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms with result: Failure - {ErrorCode}: {ErrorMessage}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    response.Error.Code,
                    response.Error.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}

/// <summary>
/// Behavior de logging detalhado (inclui serialização JSON do request).
/// Use apenas em desenvolvimento ou para debugging.
/// </summary>
public class DetailedLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ILogger<DetailedLoggingBehavior<TRequest, TResponse>> _logger;

    public DetailedLoggingBehavior(ILogger<DetailedLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        _logger.LogDebug(
            "=== START {RequestName} ===\n{RequestJson}",
            requestName,
            requestJson);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        _logger.LogDebug(
            "=== END {RequestName} ({ElapsedMilliseconds}ms) ===\n{ResponseJson}",
            requestName,
            stopwatch.ElapsedMilliseconds,
            responseJson);

        return response;
    }
}

/// <summary>
/// Behavior de performance logging (alerta se exceder threshold).
/// </summary>
public class PerformanceLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ILogger<PerformanceLoggingBehavior<TRequest, TResponse>> _logger;
    private readonly int _slowRequestThresholdMs;

    public PerformanceLoggingBehavior(
        ILogger<PerformanceLoggingBehavior<TRequest, TResponse>> logger,
        int slowRequestThresholdMs = 500)
    {
        _logger = logger;
        _slowRequestThresholdMs = slowRequestThresholdMs;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > _slowRequestThresholdMs)
        {
            _logger.LogWarning(
                "SLOW REQUEST: {RequestName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMs}ms) {@Request}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                _slowRequestThresholdMs,
                request);
        }

        return response;
    }
}

/// <summary>
/// Extensões para facilitar registro de logging behaviors.
/// </summary>
public static class LoggingBehaviorExtensions
{
    /// <summary>
    /// Registra LoggingBehavior no pipeline do MediatR.
    /// </summary>
    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }

    /// <summary>
    /// Registra DetailedLoggingBehavior (apenas desenvolvimento).
    /// </summary>
    public static IServiceCollection AddDetailedLoggingBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DetailedLoggingBehavior<,>));
        return services;
    }

    /// <summary>
    /// Registra PerformanceLoggingBehavior.
    /// </summary>
    public static IServiceCollection AddPerformanceLoggingBehavior(
        this IServiceCollection services,
        int slowRequestThresholdMs = 500)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceLoggingBehavior<,>));
        return services;
    }
}