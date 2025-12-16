namespace Bcommerce.BuildingBlocks.Application.Extensions;

public static class EnumerableExtensions
{
    // Placeholder para extensões futuras
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }
}
