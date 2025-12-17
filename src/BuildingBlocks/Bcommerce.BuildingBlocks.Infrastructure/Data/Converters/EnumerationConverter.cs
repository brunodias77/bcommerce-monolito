using Bcommerce.BuildingBlocks.Domain.Base;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Converters;

/// <summary>
/// Conversor genérico para mapear classes Enumeration para inteiros.
/// </summary>
/// <remarks>
/// Permite persistir Smart Enums como seus IDs numéricos.
/// - Converte Enumeration -> int (escrita)
/// - Converte int -> Enumeration (leitura)
/// 
/// Exemplo de uso:
/// <code>
/// builder.Property(e => e.Status).HasConversion&lt;EnumerationConverter&lt;OrderStatus&gt;&gt;();
/// </code>
/// </remarks>
public class EnumerationConverter<T> : ValueConverter<T, int>
    where T : Enumeration
{
    public EnumerationConverter() : base(
        v => v.Id,
        v => CreateState(v))
    {
    }

    private static T CreateState(int id)
    {
        return Enumeration.GetAll<T>().Single(x => x.Id == id);
    }
}
