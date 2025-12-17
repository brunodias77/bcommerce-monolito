using Bcommerce.BuildingBlocks.Application.Models;

namespace Bcommerce.BuildingBlocks.Web.Models;

/// <summary>
/// Wrapper para respostas paginadas.
/// </summary>
/// <typeparam name="T">Tipo do item na lista.</typeparam>
/// <remarks>
/// Enriquece a lista de resultados com metadados de paginação.
/// - Informa página atual, total de páginas, etc.
/// - Padrão de respostas de listagem
/// 
/// Exemplo de uso:
/// <code>
/// return new PaginatedResponse&lt;ProductDto&gt;(paginatedList);
/// </code>
/// </remarks>
public class PaginatedResponse<T>(PaginatedList<T> list)
{
    public IReadOnlyList<T> Items { get; } = list.Items;
    public int PageNumber { get; } = list.PageNumber;
    public int TotalPages { get; } = list.TotalPages;
    public int TotalCount { get; } = list.TotalCount;
    public bool HasPreviousPage { get; } = list.HasPreviousPage;
    public bool HasNextPage { get; } = list.HasNextPage;
}
