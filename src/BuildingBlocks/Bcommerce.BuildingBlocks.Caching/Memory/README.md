# Memory Cache

Este diretório contém a implementação em memória (In-Proc) da abstração de cache.

## `MemoryCacheService.cs`
**Responsabilidade:** Implementar a interface `ICacheService` utilizando o `IMemoryCache` nativo do ASP.NET Core, armazenando dados na memória RAM do próprio processo da aplicação.
**Por que existe:** Para fornecer uma opção de cache ultra-rápida (zero latência de rede) para cenários de desenvolvimento ou para aplicações monolíticas simples que não rodam em *cluster*.
**Em que situação usar:** 
1. Durante o desenvolvimento local (para não depender de subir container Docker do Redis).
2. Em produção, apenas se a aplicação rodar em uma única instância (Single Instance) ou se o dado cacheado não precisar ser compartilhado entre instâncias (ex: dados estáticos imutáveis).
**O que pode dar errado:** 
- **Inconsistência em Cluster:** Se a aplicação escalar horizontalmente (Kubernetes com 2+ replicas), cada pod terá seu próprio cache. O usuário pode salvar um dado no Pod A e tentar ler no Pod B, recebendo dados antigos ou nulos.
- **Out of Memory:** Como usa a RAM do processo, cachear objetos grandes sem controle pode estourar a memória (OOM) e derrubar a aplicação.
**Exemplo real de uso:**
```csharp
// Geralmente injetado transparente via DI:
public class ProdutoService(ICacheService cache) 
{
    // Se estiver em ambiente LOCAL, 'cache' será MemoryCacheService.
    // Se estiver em PROD, pode ser RedisCacheService (dependendo da config).
    await cache.SetAsync("chave", valor);
}
```
