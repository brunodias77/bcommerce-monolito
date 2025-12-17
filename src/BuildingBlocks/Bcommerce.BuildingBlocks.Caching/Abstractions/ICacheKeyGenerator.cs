namespace Bcommerce.BuildingBlocks.Caching.Abstractions;

/// <summary>
/// Contrato para geração padronizada de chaves de cache.
/// </summary>
/// <remarks>
/// Garante consistência na nomenclatura de chaves em toda a aplicação.
/// - Previne colisões de chaves entre diferentes contextos
/// - Facilita invalidação por padrão (ex: invalidar todas as chaves de "produto:*")
/// - Padroniza formato para facilitar debug e monitoramento
/// 
/// Exemplo de uso:
/// <code>
/// public class CacheKeyGenerator : ICacheKeyGenerator
/// {
///     public string GenerateKey(params object[] parts)
///         => string.Join(":", parts);
/// }
/// 
/// // Uso:
/// var key = _keyGenerator.GenerateKey("produto", produto.Id);
/// // Resultado: "produto:550e8400-e29b-41d4-a716-446655440000"
/// </code>
/// </remarks>
public interface ICacheKeyGenerator
{
    /// <summary>Gera uma chave de cache a partir de múltiplas partes.</summary>
    string GenerateKey(params object[] parts);
}
