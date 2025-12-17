using System.Diagnostics.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics.CustomMetrics;

/// <summary>
/// Container para métricas de performance customizadas.
/// </summary>
/// <remarks>
/// Focado em métricas técnicas não cobertas pela instrumentação padrão.
/// - Latência de operações específicas
/// - Tamanho de payloads, tempo de processamento de jobs
/// 
/// Exemplo de uso:
/// <code>
/// _performanceMetrics.CreateHistogram("job_execution_seconds").Record(1.5);
/// </code>
/// </remarks>
public class PerformanceMetrics
{
    private readonly Meter _meter;

    public PerformanceMetrics(string meterName)
    {
        _meter = new Meter(meterName);
    }

    // Add specific performance metric methods here as needed
}
