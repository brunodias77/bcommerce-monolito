# Behaviors do Pipeline (MediatR)

Este diretório contém classes que implementam a interface `IPipelineBehavior<,>` do MediatR. Elas atuam como interceptadores (Middlewares) que envelopam a execução dos handlers, permitindo aplicar lógica transversal (Cross-Cutting Concerns) de forma centralizada.

## `CachingBehavior.cs`
**Responsabilidade:** Implementar a estratégia de "Cache-Aside", interceptando a requisição para retornar dados em cache (se disponível) ou armazenar o resultado da execução (se ausente).
**Por que existe:** Para reduzir a latência de resposta e diminuir a carga sobre o banco de dados em consultas frequentes.
**Em que situação usar:** Em `Queries` que retornam dados que mudam com pouca frequência (ex: Lista de Estados, Configurações do Sistema, Catálogos). Requer mecanismo de invalidação adequado.
**O que pode dar errado:** Se a política de expiração (TTL) estiver incorreta ou não houver evicção explícita ao alterar o dado, o usuário visualizará informações obsoletas (Stale Data).
**Exemplo real de uso:**
```csharp
// 1. A Query deve implementar uma interface de marcador, ex: ICacheable
public record ObterProdutosQuery() : IRequest<Result<List<Produto>>>, ICacheable 
{
    public string CacheKey => "todos-produtos";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}

// 2. O Behavior intercepta, vê a interface e consulta o Redis antes de chamar o Handler.
```

---

## `IdempotencyBehavior.cs`
**Responsabilidade:** Garantir que uma operação de escrita não seja processada mais de uma vez para uma mesma chave de identificação.
**Por que existe:** Para manter a consistência do sistema em cenários de retentativa de rede (Network Retries) ou duplo clique do usuário.
**Em que situação usar:** Em `Commands` críticos e não-idempotentes por natureza, como "RealizarPagamento" ou "DebitarEstoque".
**O que pode dar errado:** Se o cliente (Frontend/API Gateway) recriar a chave de idempotência (ex: UUID) a cada tentativa ao invés de reutilizar a mesma para a mesma intenção, a proteção falhará e a operação será duplicada.
**Exemplo real de uso:**
```csharp
// O cliente envia o Header: `X-Idempotency-Key: 123e4567-e89b...`

// O Behavior verifica no banco se "123e4567..." já foi processado.
// Se SIM: Retorna o resultado salvo anteriormente (sem executar o handler).
// Se NÃO: Executa o handler e salva o resultado associado à chave.
```

---

## `LoggingBehavior.cs`
**Responsabilidade:** Registrar logs estruturados contendo o nome do comando, dados do payload (Request) e informações do usuário no início e fim do processamento.
**Por que existe:** Para fornecer rastreabilidade e auditoria, permitindo entender qual comando foi executado, por quem e com quais dados.
**Em que situação usar:** Globalmente, registrado para todos os requests do MediatR.
**O que pode dar errado:** Vazamento de dados sensíveis (PII, Senhas, Cartões) nos logs se não houver um filtro de "Destructuring" configurado no Logger (Serilog) ou no próprio Behavior.
**Exemplo real de uso:**
```csharp
// Output automático nos logs do console/Seq:

// [INF] Processando requisição: CriarPedidoCommand { ClienteId: "Guid", Valor: 100.00 }
await next(); 
// [INF] Requisição processada: CriarPedidoCommand
```

---

## `PerformanceBehavior.cs`
**Responsabilidade:** Cronometrar o tempo de execução do pipeline (Handler + outros Behaviors) e emitir alertas se exceder um limite configurado.
**Por que existe:** Para detectar proativamente degradação de performance e identificar "Slow Queries" ou lógicas de negócio lentas.
**Em que situação usar:** Globalmente em produção.
**O que pode dar errado:** Definir um limiar muito baixo (ex: 10ms) pode gerar "Noise" (excesso de logs inúteis), dificultando a identificação de problemas reais.
**Exemplo real de uso:**
```csharp
// Configuração interna: Limiar de 500ms.

// Se o handler "RelatorioVendasHandler" levar 800ms:
// [WRN] Longa Duração na Requisição: RelatorioVendasQuery (800 milissegundos)
```

---

## `TransactionBehavior.cs`
**Responsabilidade:** Gerenciar o escopo de transação de banco de dados (Begin, Commit, Rollback) ao redor da execução do Handler.
**Por que existe:** Para garantir a atomicidade (ACID). Assegura que todas as modificações feitas no banco durante o Handler sejam persistidas juntas ou descartadas em caso de erro.
**Em que situação usar:** Exclusivamente em `Commands` (operações de escrita). Não deve ser usado em Queries.
**O que pode dar errado:** Se o Handler realizar chamadas externas demoradas (HTTP requests) dentro da transação, manterá conexões e locks de banco presos, podendo causar Deadlocks e esgotar o Pool de Conexões.
**Exemplo real de uso:**
```csharp
// O Behavior abre a transação (BeginTransaction)
try {
    await next(); // O Handler salva Pedido e ItensPedido
    // O Behavior faz Commit
} catch {
    // O Behavior faz Rollback
}
```

---

## `ValidationBehavior.cs`
**Responsabilidade:** Executar validações de entrada (FluentValidation) antes de invocar o Handler, interrompendo o pipeline em caso de falha (Fail-Fast).
**Por que existe:** Para garantir que o Handler receba apenas objetos válidos, removendo a poluição de `if (string.IsNullOrEmpty)` da lógica de negócio.
**Em que situação usar:** Em todos os `Commands` que requerem validação de formato, tamanho ou obrigatoriedade de campos.
**O que pode dar errado:** Implementar regras de negócio dependentes de banco de dados (ex: "NomeDeUsuarioJaExiste") no Validator. Isso deve ser feito no Domain Service ou Handler, pois Validators devem ser leves e síncronos (ou rápidos).
**Exemplo real de uso:**
```csharp
public class CriarClienteValidator : AbstractValidator<CriarClienteCommand> 
{
    public CriarClienteValidator() {
        RuleFor(x => x.Email).EmailAddress();
    }
}

// Se o email for inválido, o ValidationBehavior lança ValidationException.
// O Handler `CriarClienteHandler` NEM CHEGA a ser instanciado/executado.
```
