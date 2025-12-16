namespace Bcommerce.BuildingBlocks.Application.Models;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        // Nota: Em uma implementação real com EF Core, usaria ToListAsync.
        // Como aqui é BuildingBlocks e não queremos depedência direta do EF Core,
        // vamos assumir que o IQueryable pode ser materializado ou será tratado na infra.
        // Para fins deste arquivo, faremos a lógica básica.
        
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
