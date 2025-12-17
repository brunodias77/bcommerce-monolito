# Redis Cache

Este diretório contém a implementação do provedor de cache distribuído utilizando Redis, projetado para ambientes escaláveis e de produção.

## `RedisCacheService.cs`
**Responsabilidade:** Implementar a interface `ICacheService` utilizando o Redis como armazenamento, através da biblioteca `IDistributedCache` e serialização JSON.
**Por que existe:** Para permitir que múltiplas instâncias da aplicação (escalabilidade horizontal) compartilhem o mesmo cache, garantindo consistência de dados em toda a frota de servidores.
**Em que situação usar:** Em ambientes de Produção e Homologação onde a aplicação roda em containers/pods distribuídos.
**O que pode dar errado:** 
- **Latency Spikes:** Se o servidor Redis estiver sobrecarregado ou longe fisicamente, o tempo de resposta da aplicação aumentará.
- **Serialization Issues:** Objetos com referência circular ou tipos não serializáveis irão lançar exceções ao tentar salvar no Redis.
**Exemplo real de uso:**
```csharp
// Injetado automaticamente quando "RedisSettings:ConnectionString" está presente.
public class CatalogoService(ICacheService cache)
{
    public async Task<Produto> Obter(Guid id)
    {
        // Busca no servidor Redis compartilhado
        var produto = await cache.GetAsync<Produto>($"prod:{id}");
    }
}
```

---

## `RedisSettings.cs`
**Responsabilidade:** Mapear as configurações necessárias para conexão com o Redis a partir do arquivo `appsettings.json`.
**Por que existe:** Para tipar as configurações (Strongly Typed Configuration) e evitar leitura direta de strings do `IConfiguration` espalhadas pelo código.
**Em que situação usar:** Automaticamente utilizado na inicialização da aplicação (`ServiceCollectionExtensions`) para configurar o cliente Redis.
**O que pode dar errado:** Se a `ConnectionString` estiver mal formatada ou a senha incorreta, a aplicação falhará ao tentar conectar no Redis (dependendo da política de retry).
**Exemplo real de uso:**
```json
// appsettings.json
"RedisSettings": {
    "ConnectionString": "meu-redis.azure.com:6380,password=xyz,ssl=true",
    "InstanceNumber": 1
}
```
