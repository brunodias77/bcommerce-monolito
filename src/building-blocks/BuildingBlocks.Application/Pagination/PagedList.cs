using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Pagination;

/// <summary>
/// Implementação de lista paginada
/// Suporta paginação eficiente usando OFFSET/LIMIT do PostgreSQL
/// </summary>
/// <typeparam name="T">Tipo dos itens na lista</typeparam>
public sealed class PagedList<T> : IPaginatedResult<T>
{
    private PagedList(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        Items = items;
        CurrentPage = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int CurrentPage { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => CurrentPage > 1;

    public bool HasNext => CurrentPage < TotalPages;

    /// <summary>
    /// Cria uma lista paginada vazia
    /// </summary>
    public static PagedList<T> Empty(int pageNumber = 1, int pageSize = 10) =>
        new(Array.Empty<T>(), pageNumber, pageSize, 0);

    /// <summary>
    /// Cria uma lista paginada a partir de uma query IQueryable
    /// Executa COUNT(*) e OFFSET/LIMIT de forma eficiente no PostgreSQL
    /// </summary>
    /// <param name="query">Query base (sem Skip/Take aplicados)</param>
    /// <param name="pageNumber">Número da página (começa em 1)</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Valida parâmetros
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        if (pageSize > 100)
        {
            pageSize = 100; // Limite máximo para evitar queries muito grandes
        }

        // Conta total de registros
        var totalCount = await query.CountAsync(cancellationToken);

        // Se não há registros, retorna lista vazia
        if (totalCount == 0)
        {
            return Empty(pageNumber, pageSize);
        }

        // Busca itens da página atual usando OFFSET/LIMIT
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Cria uma lista paginada a partir de uma lista em memória
    /// Útil para testes ou quando os dados já foram carregados
    /// </summary>
    public static PagedList<T> Create(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var items = source.ToList();
        var totalCount = items.Count;

        var pagedItems = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedList<T>(pagedItems, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Mapeia os itens para outro tipo
    /// </summary>
    public PagedList<TOutput> Map<TOutput>(Func<T, TOutput> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedList<TOutput>(mappedItems, CurrentPage, PageSize, TotalCount);
    }
}
