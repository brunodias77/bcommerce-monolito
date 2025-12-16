using Bcommerce.BuildingBlocks.Application.Models;

namespace Bcommerce.BuildingBlocks.Application.Extensions;

public static class QueryableExtensions
{
    public static Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        return PaginatedList<T>.CreateAsync(source, pageNumber, pageSize);
    }
}
