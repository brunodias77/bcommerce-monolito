using System.Diagnostics;

namespace Bcommerce.BuildingBlocks.Observability.Tracing;

/// <summary>
/// Métodos de extensão para facilitar o uso de System.Diagnostics.Activity.
/// </summary>
/// <remarks>
/// Utilitários para enriquecer spans de trace.
/// - Adiciona tags de forma segura (verificando null)
/// - Simplifica a instrumentação manual no código de negócio
/// 
/// Exemplo de uso:
/// <code>
/// Activity.Current?.SetTagIfPresent("user.id", userId);
/// </code>
/// </remarks>
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
