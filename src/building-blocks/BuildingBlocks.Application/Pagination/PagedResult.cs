namespace BuildingBlocks.Application.Pagination;

/// <summary>
/// Resultado paginado genérico.
/// </summary>
/// <typeparam name="T">Tipo dos itens</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Itens da página atual.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Número da página atual (1-based).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Tamanho da página.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Total de itens em todas as páginas.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Total de páginas disponíveis.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Indica se existe página anterior.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se existe próxima página.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Indica se esta é a primeira página.
    /// </summary>
    public bool IsFirstPage => PageNumber == 1;

    /// <summary>
    /// Indica se esta é a última página.
    /// </summary>
    public bool IsLastPage => PageNumber == TotalPages;

    private PagedResult(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Cria um resultado paginado.
    /// </summary>
    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Cria um resultado paginado vazio.
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PagedResult<T>(
            Array.Empty<T>(),
            pageNumber,
            pageSize,
            0);
    }

    /// <summary>
    /// Mapeia os itens para outro tipo.
    /// </summary>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedResult<TResult>(
            mappedItems,
            PageNumber,
            PageSize,
            TotalCount);
    }

    /// <summary>
    /// Informações de metadados da paginação.
    /// </summary>
    public PaginationMetadata GetMetadata()
    {
        return new PaginationMetadata(
            PageNumber,
            PageSize,
            TotalCount,
            TotalPages,
            HasPreviousPage,
            HasNextPage);
    }
}

/// <summary>
/// Metadados de paginação (útil para APIs REST).
/// </summary>
public sealed record PaginationMetadata(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);


