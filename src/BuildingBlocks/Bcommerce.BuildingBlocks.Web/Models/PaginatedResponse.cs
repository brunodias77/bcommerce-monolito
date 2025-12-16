using Bcommerce.BuildingBlocks.Application.Models;

namespace Bcommerce.BuildingBlocks.Web.Models;

public class PaginatedResponse<T>(PaginatedList<T> list)
{
    public IReadOnlyList<T> Items { get; } = list.Items;
    public int PageNumber { get; } = list.PageNumber;
    public int TotalPages { get; } = list.TotalPages;
    public int TotalCount { get; } = list.TotalCount;
    public bool HasPreviousPage { get; } = list.HasPreviousPage;
    public bool HasNextPage { get; } = list.HasNextPage;
}
