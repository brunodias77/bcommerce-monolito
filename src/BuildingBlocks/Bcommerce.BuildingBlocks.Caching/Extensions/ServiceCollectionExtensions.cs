using Bcommerce.BuildingBlocks.Caching.Abstractions;
using Bcommerce.BuildingBlocks.Caching.Memory;
using Bcommerce.BuildingBlocks.Caching.Redis;
using Bcommerce.BuildingBlocks.Caching.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.BuildingBlocks.Caching.Extensions;

/// <summary>
/// Extensões para configuração de serviços de cache no container de DI.
/// </summary>
/// <remarks>
/// Configura automaticamente Redis ou MemoryCache baseado nas configurações.
/// - Se ConnectionString do Redis estiver configurada: usa Redis
/// - Caso contrário: usa MemoryCache (ideal para desenvolvimento)
/// - Registra estratégias de cache (CacheAside, Invalidation)
/// 
/// Exemplo de uso:
/// <code>
/// // Em Program.cs ou Startup.cs:
/// builder.Services.AddCachingServices(builder.Configuration);
/// 
/// // appsettings.json:
/// {
///   "RedisSettings": {
///     "ConnectionString": "localhost:6379"
///   }
/// }
/// </code>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona serviços de cache ao container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <returns>Coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisSettings = new RedisSettings();
        configuration.Bind(RedisSettings.SectionName, redisSettings);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(redisSettings));

        if (!string.IsNullOrEmpty(redisSettings.ConnectionString) && redisSettings.ConnectionString != "localhost")
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.ConnectionString;
                options.InstanceName = "Bcommerce_";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        services.AddScoped<CacheAsideStrategy>();
        services.AddScoped<CacheInvalidationStrategy>();

        return services;
    }
}
