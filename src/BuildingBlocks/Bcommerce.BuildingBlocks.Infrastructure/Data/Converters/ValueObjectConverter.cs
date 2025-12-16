using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Converters;

// Converter genérico para armazenar objetos de valor complexos como JSON se necessário.
// EF Core 8+ já tem suporte melhor a JSON columns, mas este é um fallback útil.

public class ValueObjectConverter<T>() : ValueConverter<T, string>(
    v => JsonConvert.SerializeObject(v),
    v => JsonConvert.DeserializeObject<T>(v)!)
    where T : class
{
}
