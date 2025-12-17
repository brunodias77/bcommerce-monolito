using System.Diagnostics;

namespace Bcommerce.BuildingBlocks.Observability.Tracing;

public static class ActivityExtensions
{
    public static void SetTagIfPresent(this Activity? activity, string key, object? value)
    {
        if (value != null)
        {
            activity?.SetTag(key, value);
        }
    }
}
