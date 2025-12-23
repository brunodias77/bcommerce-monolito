namespace Bcommerce.BuildingBlocks.Application.Extensions;

/// <summary>
/// Métodos de extensão para coleções <see cref="IEnumerable{T}"/>.
/// Fornece utilitários comuns para manipulação de coleções na camada de aplicação.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Verifica se uma coleção é nula ou vazia.
    /// </summary>
    /// <typeparam name="T">Tipo dos elementos da coleção.</typeparam>
    /// <param name="source">Coleção a ser verificada.</param>
    /// <returns>True se a coleção for nula ou não contiver elementos; False caso contrário.</returns>
    /// <remarks>
    /// Verifica se a coleção é nula ou não contém elementos.
    /// - Evita NullReferenceException ao iterar
    /// - Condensa duas verificações comuns em uma chamada
    /// - Útil para guard clauses
    /// 
    /// Exemplo de uso:
    /// <code>
    /// if (pedidos.IsNullOrEmpty())
    /// {
    ///     return Result.Failure("Nenhum pedido encontrado");
    /// }
    /// </code>
    /// </remarks>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }
}
