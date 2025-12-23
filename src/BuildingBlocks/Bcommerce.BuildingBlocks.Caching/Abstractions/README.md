# Abstrações de Cache

Este diretório contém os contratos que definem como a aplicação interage com mecanismos de cache (seja em memória, Redis ou outros), garantindo desacoplamento da infraestrutura.

## `ICacheKeyGenerator.cs`
**Responsabilidade:** Padronizar a criação de strings que funcionam como chaves únicas para itens no cache.
**Por que existe:** Para evitar "Magic Strings" espalhadas pelo código e garantir que diferentes partes do sistema (ex: Invalidação e Leitura) gerem exatamente a mesma chave para o mesmo recurso, além de prevenir colisões entre entidades diferentes.
**Em que situação usar:** Sempre que for acessar o cache, use este gerador para criar a chave baseada nos parâmetros do objeto (ID, Categoria, etc).
**O que pode dar errado:** Se cada desenvolvedor concatenar strings manualmente (`$"prod:{id}"` vs `$"product-{id}"`), haverá fragmentação do cache e bugs onde um serviço invalidou a chave A mas a leitura buscou a chave B.
**Exemplo real de uso:**
```csharp
// Injeção
public class ProdutoService(ICacheKeyGenerator keyGen, ICacheService cache) {
    public async Task<Produto> Obter(Guid id) {
        // Gera: "bcommerce:catalogo:produto:123-abc" de forma consistente
        string key = keyGen.GenerateKey("catalogo", "produto", id);
        return await cache.GetAsync<Produto>(key);
    }
}
```

---

## `ICacheService.cs`
**Responsabilidade:** Prover uma interface unificada para operações básicas de cache (Get, Set, Remove, Refresh), abstraindo o provider subjacente (Redis, In-Memory, Memcached).
**Por que existe:** Para permitir que a aplicação mude de tecnologia de cache (ex: começar com MemoryCache e migrar para Redis Cluster) sem alterar uma única linha de código de negócio.
**Em que situação usar:** Em Handlers (Queries), Services ou Decorators que precisam armazenar dados temporários para performance.
**O que pode dar errado:** Armazenar objetos gigantescos ou grafos cíclicos sem DTOs específicos, causando estouro de memória ou lentidão na serialização. Esquecer de definir expiração (`expiration`), criando "Memory Leaks" no Redis (chaves eternas).
**Exemplo real de uso:**
```csharp
var produto = await _cacheService.GetAsync<ProdutoDto>(key);

if (produto is null) {
    produto = await _repo.GetById(id);
    // Salva por 10 minutos (Cache-Aside pattern)
    await _cacheService.SetAsync(key, produto, TimeSpan.FromMinutes(10));
}
```
