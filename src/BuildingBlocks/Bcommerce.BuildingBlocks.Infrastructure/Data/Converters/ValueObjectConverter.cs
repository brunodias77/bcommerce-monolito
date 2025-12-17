using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Converters;

// Converter genérico para armazenar objetos de valor complexos como JSON se necessário.
// EF Core 8+ já tem suporte melhor a JSON columns, mas este é um fallback útil.

/// <summary>
/// Conversor genérico para serializar Objetos de Valor como JSON.
/// </summary>
/// <remarks>
/// Facilita a persistência de objetos complexos em uma única coluna texto.
/// - Serializa para JSON na escrita
/// - Deserializa do JSON na leitura
/// - Útil para Value Objects sem tabela própria
/// 
/// Exemplo de uso:
/// <code>
/// builder.Property(e => e.Address).HasConversion&lt;ValueObjectConverter&lt;Address&gt;&gt;();
/// </code>
/// </remarks>
public class ValueObjectConverter<T>() : ValueConverter<T, string>(
    v => JsonConvert.SerializeObject(v),
    v => JsonConvert.DeserializeObject<T>(v)!)
    where T : class
{
}
