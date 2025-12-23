using Bcommerce.BuildingBlocks.Application.Abstractions.Services;

namespace Bcommerce.BuildingBlocks.Infrastructure.Time;

/// <summary>
/// Provedor de data e hora para a infraestrutura.
/// </summary>
/// <remarks>
/// Encapsula o acesso a DateTime.UtcNow.
/// - Implementação real para execução em produção
/// - Usado para garantir consistência temporal entre componentes
/// 
/// Exemplo de uso:
/// <code>
/// services.AddScoped&lt;IDateTimeProvider, DateTimeProvider&gt;();
/// </code>
/// </remarks>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
