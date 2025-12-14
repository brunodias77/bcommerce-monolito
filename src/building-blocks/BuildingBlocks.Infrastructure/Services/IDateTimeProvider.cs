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
/// Implementação fake para testes (permite fixar data/hora).
/// </summary>
public class FakeDateTimeProvider : IDateTimeProvider
{
    private DateTime _fixedDateTime;

    public FakeDateTimeProvider(DateTime? fixedDateTime = null)
    {
        _fixedDateTime = fixedDateTime ?? DateTime.UtcNow;
    }

    public DateTime UtcNow => _fixedDateTime;

    public DateTime Now => _fixedDateTime.ToLocalTime();

    public DateTime Today => _fixedDateTime.Date;

    /// <summary>
    /// Define uma nova data/hora fixa.
    /// </summary>
    public void SetDateTime(DateTime dateTime)
    {
        _fixedDateTime = dateTime;
    }

    /// <summary>
    /// Avança o tempo em um intervalo específico.
    /// </summary>
    public void Advance(TimeSpan interval)
    {
        _fixedDateTime = _fixedDateTime.Add(interval);
    }

    /// <summary>
    /// Reseta para a data/hora atual.
    /// </summary>
    public void Reset()
    {
        _fixedDateTime = DateTime.UtcNow;
    }
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

    /// <summary>
    /// Registra um DateTimeProvider fake para testes.
    /// </summary>
    public static IServiceCollection AddFakeDateTimeProvider(
        this IServiceCollection services,
        DateTime? fixedDateTime = null)
    {
        services.AddSingleton<IDateTimeProvider>(new FakeDateTimeProvider(fixedDateTime));
        return services;
    }
}