using System.Diagnostics.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics.CustomMetrics;

public class PerformanceMetrics
{
    private readonly Meter _meter;

    public PerformanceMetrics(string meterName)
    {
        _meter = new Meter(meterName);
    }

    // Add specific performance metric methods here as needed
}
