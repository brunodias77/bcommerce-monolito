namespace Bcommerce.BuildingBlocks.Caching.Redis;

/// <summary>
/// Configurações de conexão com o servidor Redis.
/// </summary>
/// <remarks>
/// Mapeada a partir da seção "RedisSettings" do appsettings.json.
/// - ConnectionString suporta formato Redis padrão (host:port,password=xxx)
/// - Se não configurada, o sistema usa MemoryCache automaticamente
/// - InstanceNumber usado para numerar instâncias em clusters
/// 
/// Exemplo de uso:
/// <code>
/// // appsettings.json:
/// {
///   "RedisSettings": {
///     "ConnectionString": "redis.exemplo.com:6379,password=minhasenha,ssl=true",
///     "InstanceNumber": 1
///   }
/// }
/// </code>
/// </remarks>
public class RedisSettings
{
    /// <summary>Nome da seção no arquivo de configuração.</summary>
    public const string SectionName = "RedisSettings";

    /// <summary>String de conexão com o Redis.</summary>
    public string ConnectionString { get; set; } = "localhost";
    /// <summary>Número da instância para identificação em clusters.</summary>
    public int InstanceNumber { get; set; } = -1;
}
