namespace Bcommerce.BuildingBlocks.Application.Models;

public record PagedRequest(int PageNumber = 1, int PageSize = 10, string? SortBy = null, bool IsAscending = true)
{
    public int PageNumber { get; init; } = PageNumber < 1 ? 1 : PageNumber;
    public int PageSize { get; init; } = PageSize > 100 ? 100 : (PageSize < 1 ? 10 : PageSize);
}
