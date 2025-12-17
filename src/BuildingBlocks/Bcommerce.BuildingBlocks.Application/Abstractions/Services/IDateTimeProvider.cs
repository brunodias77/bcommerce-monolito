namespace Bcommerce.BuildingBlocks.Application.Abstractions.Services;

/// <summary>
/// Abstração para obtenção de data/hora do sistema.
/// </summary>
/// <remarks>
/// Essencial para testabilidade de lógicas sensíveis ao tempo.
/// - Permite mocks determinísticos em testes unitários (Time Travel)
/// - Garante uso consistente de UTC em toda a aplicação
/// - Evita chamadas diretas a DateTime.Now/UtcNow
/// 
/// Exemplo de uso:
/// <code>
/// // Na aplicação:
/// var pedido = new Pedido(_dateTimeProvider.UtcNow);
/// 
/// // No teste:
/// mock.Setup(x => x.UtcNow).Returns(new DateTime(2023, 1, 1));
/// </code>
/// </remarks>
public interface IDateTimeProvider
{
    /// <summary>Obtém a data e hora atual em UTC.</summary>
    DateTime UtcNow { get; }
}
