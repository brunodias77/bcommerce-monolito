using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Services;

/// <summary>
/// Interface para prover data/hora atual (útil para testes).
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Retorna a data/hora atual em UTC.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Retorna a data/hora atual local.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Retorna a data atual (sem hora).
    /// </summary>
    DateTime Today { get; }
}

/// <summary>
/// Implementação padrão que usa DateTime.UtcNow.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => DateTime.Now;

    public DateTime Today => DateTime.Today;
}

/// <summary>
/// Extensões para facilitar registro do DateTimeProvider.
/// </summary>
public static class DateTimeProviderExtensions
{
    /// <summary>
    /// Registra o DateTimeProvider padrão.
    /// </summary>
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}