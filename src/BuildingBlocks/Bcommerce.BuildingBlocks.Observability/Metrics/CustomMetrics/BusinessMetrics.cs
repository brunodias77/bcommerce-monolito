using System.Diagnostics.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics.CustomMetrics;

/// <summary>
/// Container para métricas de negócio customizadas.
/// </summary>
/// <remarks>
/// Encapsula a criação de contadores e histogramas de domínio.
/// - Usado para rastrear KPIs (Vendas, Cadastros, Erros de negócio)
/// - Abstrai a criação direta de objetos Meter
/// 
/// Exemplo de uso:
/// <code>
/// _businessMetrics.CreateCounter("orders_placed_total").Add(1);
/// </code>
/// </remarks>
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
