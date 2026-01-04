using BuildingBlocks.Application.Interfaces;

namespace BuildingBlocks.Infrastructure.Services;

/// <summary>
/// Implementação real do provedor de data e hora
/// Retorna a data/hora atual do sistema
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Obtém a data e hora atual em UTC
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Obtém a data e hora atual no fuso horário local
    /// </summary>
    public DateTime Now => DateTime.Now;

    /// <summary>
    /// Obtém apenas a data atual (sem hora)
    /// </summary>
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    /// Obtém apenas a hora atual (sem data)
    /// </summary>
    public TimeOnly TimeOfDay => TimeOnly.FromDateTime(DateTime.Now);
}