using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Converters;

/// <summary>
/// Conversor de valor do EF Core para garantir DateTime em UTC.
/// </summary>
/// <remarks>
/// Normaliza datas para UTC ao ler e gravar no banco.
/// - Previne problemas de fuso horário
/// - Força `Kind=Utc` ao ler do banco
/// 
/// Exemplo de uso:
/// <code>
/// builder.Property(e => e.Date).HasConversion&lt;DateTimeUtcConverter&gt;();
/// </code>
/// </remarks>
public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter() : base(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
