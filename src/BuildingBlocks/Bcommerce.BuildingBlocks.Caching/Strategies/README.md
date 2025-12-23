# Caching Strategies

Este diretório contém classes que封装m padrões arquiteturais de uso de cache, facilitando a implementação de regras de negócio consistentes.

## `CacheAsideStrategy.cs`
**Responsabilidade:** Implementar o padrão **Cache-Aside (Lazy Loading)**, onde a aplicação é responsável por carregar os dados no cache sob demanda.
**Por que existe:** Para encapsular o boilerplate de "Verifica Cache -> Se Null -> Busca no Banco -> Salva no Cache -> Retorna". Isso evita repetir esse bloco `if/else` em todos os Handlers.
**Em que situação usar:** Em Queries e Serviços de Leitura para dados que podem ser cacheados. É a estratégia padrão para leitura.
**O que pode dar errado:** 
- **Cache Penetration:** Se a *factory* retornar `null` (item não encontrado no banco) e não salvarmos esse "nulo" no cache (com expiração curta), requisições maliciosas por IDs inexistentes continuarão batendo no banco repetidamente.
- **Thundering Herd:** Se muitos usuários pedirem a mesma chave expirada simultaneamente, todos passarão pelo cache e baterão no banco ao mesmo tempo antes que o primeiro consiga repopular o cache.
**Exemplo real de uso:**
```csharp
// Em vez de escrever if(cache == null) manualmente:
var produto = await _cacheAside.GetOrCreateAsync(
    key: $"prod:{id}",
    factory: () => _repo.GetByIdAsync(id), // Só executa se não achar no cache
    expiration: TimeSpan.FromMinutes(30)
);
```

---

## `CacheInvalidationStrategy.cs`
**Responsabilidade:** Encapsular a lógica de remoção (evicção) de itens do cache para garantir consistência após alterações.
**Por que existe:** Para tornar explícito no código onde e quando ocorre a invalidação, promovendo o padrão de que "quem altera o dado é responsável por invalidar o cache dele".
**Em que situação usar:** Em Commands (Create, Update, Delete) imediatamente após a persistência bem-sucedida no banco de dados.
**O que pode dar errado:** 
- **Inconsistência:** Se a transação do banco comitar mas o Redis cair antes da invalidação (erro de rede), o cache ficará com dados antigos ("stale") até expirar naturalmente. Nesses casos, o ideal seria usar mensagens (Events) para tentar invalidar novamente.
- **Race Conditions:** Em sistemas muito concorridos, um Update pode invalidar o cache milissegundos antes de uma Query antiga repopular o cache com o dado velho.
**Exemplo real de uso:**
```csharp
// Após UPDATE no banco:
await _repo.UpdateAsync(produto);

// Garante que a próxima leitura busque o dado fresco
await _invalidation.InvalidateAsync($"prod:{produto.Id}");
```
