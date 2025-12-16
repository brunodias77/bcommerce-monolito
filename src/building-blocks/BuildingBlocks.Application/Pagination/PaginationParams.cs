namespace BuildingBlocks.Application.Pagination;

/// <summary>
/// Parâmetros de paginação para queries.
/// </summary>
/// <remarks>
/// Uso em Queries:
/// <code>
/// public record GetProductsQuery(PaginationParams Pagination) : IQuery&lt;PagedResult&lt;ProductDto&gt;&gt;;
/// 
/// // Handler
/// var products = await _repository.GetAll()
///     .Skip(pagination.Skip)
///     .Take(pagination.PageSize)
///     .ToListAsync();
/// </code>
/// 
/// Uso em Controllers:
/// <code>
/// [HttpGet]
/// public async Task&lt;IActionResult&gt; GetAll([FromQuery] PaginationParams pagination)
/// {
///     var result = await Mediator.Send(new GetProductsQuery(pagination));
///     return HandleResult(result);
/// }
/// </code>
/// </remarks>
public sealed class PaginationParams
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Número da página (1-based). Default: 1
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        // Proteção: Garante que a página nunca seja menor que 1
        set => _pageNumber = value < 1 ? DefaultPageNumber : value;
    }

    /// <summary>
    /// Tamanho da página. Default: 10, Max: 100
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        // Proteção: Clamp entre 1 e MaxPageSize (100)
        // Evita sobrecarga do banco de dados com pages muito grandes
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>
    /// Número de itens a pular (para Skip no LINQ).
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Ordenação (ex: "name", "createdAt desc").
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Termo de busca opcional.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Cria parâmetros de paginação com valores customizados.
    /// </summary>
    public static PaginationParams Create(int pageNumber = 1, int pageSize = 10, string? orderBy = null)
    {
        return new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            OrderBy = orderBy
        };
    }

    /// <summary>
    /// Parâmetros padrão (página 1, 10 itens).
    /// </summary>
    public static PaginationParams Default => new();

    /// <summary>
    /// Primeira página com tamanho especificado.
    /// </summary>
    public static PaginationParams FirstPage(int pageSize = 10) => new() { PageSize = pageSize };

    /// <summary>
    /// Todos os itens (usar com cautela - sem paginação real).
    /// </summary>
    public static PaginationParams All => new() { PageSize = MaxPageSize };
}
