using System.Diagnostics.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics.CustomMetrics;

public class BusinessMetrics
{
    private readonly Meter _meter;

    public BusinessMetrics(string meterName)
    {
        _meter = new Meter(meterName);
    }

    public Counter<long> CreateCounter(string name, string? unit = null, string? description = null)
    {
        return _meter.CreateCounter<long>(name, unit, description);
    }

    public Histogram<double> CreateHistogram(string name, string? unit = null, string? description = null)
    {
        return _meter.CreateHistogram<double>(name, unit, description);
    }
}
