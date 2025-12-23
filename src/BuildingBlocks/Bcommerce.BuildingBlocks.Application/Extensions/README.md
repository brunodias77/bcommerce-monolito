# Extensions da Aplicação

Este diretório contém classes estáticas com métodos de extensão (Extension Methods) que adicionam funcionalidades úteis a tipos comuns do .NET e do próprio domínio da aplicação. Elas visam reduzir a verbosidade e promover a reutilização de código padrão.

## `EnumerableExtensions.cs`
**Responsabilidade:** Fornecer métodos utilitários para coleções em memória (`IEnumerable<T>`) de forma segura e expressiva.
**Por que existe:** Para encapsular verificações repetitivas de nulidade ou estado da lista, evitando `NullReferenceException` e condicionais verbosas.
**Em que situação usar:** Sempre que precisar verificar se uma lista é nula OU vazia em uma única instrução, especialmente em Guard Clauses ou validações.
**O que pode dar errado:** Não existe muito risco, mas o uso excessivo em "Hot Paths" (loops críticos) pode ter um custo ínfimo de performance se comparado a checagens manuais diretas (mas geralmente irrelevante).
**Exemplo real de uso:**
```csharp
IEnumerable<Pedido> pedidos = null;

// Sem a extension:
if (pedidos == null || !pedidos.Any()) { ... }

// Com a extension:
if (pedidos.IsNullOrEmpty()) { 
    return Result.Failure("Nenhum pedido encontrado.");
}
```

---

## `QueryableExtensions.cs`
**Responsabilidade:** Adicionar funcionalidades a consultas LINQ que ainda serão traduzidas para SQL (`IQueryable<T>`), principalmente paginação.
**Por que existe:** Para permitir que a lógica de paginação (Skip/Take e Contagem) seja aplicada diretamente no banco de dados de forma reutilizável, sem duplicar código em cada Query Handler.
**Em que situação usar:** Ao retornar listas paginadas do banco de dados (EF Core) para a API.
**O que pode dar errado:** Executar métodos que materializam a consulta (como `.ToList()`) **antes** de chamar o `.ToPaginatedListAsync()`. Isso traria a tabela inteira para a memória antes de paginar, matando a performance.
**Exemplo real de uso:**
```csharp
// No Handler de consulta:
var query = _context.Produtos.Where(p => p.Ativo);

// Aplica paginação no banco e retorna PaginatedList<Produto>:
var resultado = await query.ToPaginatedListAsync(request.Page, request.PageSize);
```

---

## `ResultExtensions.cs`
**Responsabilidade:** Habilitar o estilo de programação "Railway-Oriented Programming" (ROP) para a classe `Result`. Adiciona métodos como `Bind` para encadeamento de operações.
**Por que existe:** Para reduzir a complexidade ciclomática de `if (sucesso) { ... } else { return erro; }` aninhados. Permite ler o fluxo de negócio como uma sequência linear de passos.
**Em que situação usar:** Em fluxos de negócio complexos onde a saída de um passo é a entrada do próximo, e qualquer falha no meio deve abortar o processo imediatamente.
**O que pode dar errado:** Criar cadeias muito longas e difíceis de depurar. Se uma exceção não tratada ocorrer dentro de um `Bind`, o fluxo quebra. É importante manter o equilíbrio e não forçar ROP em tudo.
**Exemplo real de uso:**
```csharp
// O fluxo para no primeiro erro encontrado:
return await ValidarPedido(pedido)
    .Bind(VerificarEstoque)         // Só executa se ValidarPedido for Sucesso
    .Bind(ReservarEstoque)          // Só executa se VerificarEstoque for Sucesso
    .Bind(ProcessarPagamento);      // Só executa se ReservarEstoque for Sucesso
```
