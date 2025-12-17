using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Bcommerce.BuildingBlocks.Observability.Logging.SerilogEnrichers;

/// <summary>
/// Enriquecedor do Serilog para adicionar o TraceIdentifier (CorrelationId) aos logs.
/// </summary>
/// <remarks>
/// Extrai o ID de correlação do HttpContext.
/// - Permite rastrear requisições através de múltiplos logs
/// - Adiciona a propriedade "CorrelationId" se disponível
/// 
/// Exemplo de uso:
/// <code>
/// .Enrich.With&lt;CorrelationIdEnricher&gt;()
/// </code>
/// </remarks>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher() : this(new HttpContextAccessor())
    {
    }

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        var property = propertyFactory.CreateProperty("CorrelationId", correlationId);
        logEvent.AddPropertyIfAbsent(property);
    }
}
