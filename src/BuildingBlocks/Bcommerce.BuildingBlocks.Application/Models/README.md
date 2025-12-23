# Models da Aplicação

Este diretório contém os objetos de valor (Value Objects) e estruturas fundamentais que dão suporte à comunicação e fluxo de dados dentro da camada de aplicação. Eles padronizam como erros, resultados e requisições complexas são representados.

## `Error.cs`
**Responsabilidade:** Representar um erro de domínio de forma estruturada, imutável e desacoplada de exceções.
**Por que existe:** Para permitir que métodos retornem falhas como parte do fluxo normal de controle (Result Pattern) sem o custo de performance de lançar exceções para validações de regra de negócio.
**Em que situação usar:** Ao criar retornos de falha para métodos que retornam `Result`. Use os métodos de fábrica estáticos (ex: `Error.Validation`, `Error.Conflict`).
**O que pode dar errado:** Criar erros com códigos genéricos ("Error") que não ajudam o frontend a traduzir a mensagem. Use códigos específicos ("User.EmailNotFound").
**Exemplo real de uso:**
```csharp
if (produto.Estoque < quantidade) {
    return Result.Failure(Error.Validation("Cart.InsufficientStock", "Estoque insuficiente."));
}
```

---

## `ErrorType.cs`
**Responsabilidade:** Enumerar as categorias macro de erros possíveis (Validação, NotFound, Conflito, etc.).
**Por que existe:** Para permitir que a camada de apresentação (API) converta automaticamente um erro de domínio no Status Code HTTP correto (400, 404, 409) sem acoplar o Domínio ao ASP.NET Core.
**Em que situação usar:** Automaticamente preenchido ao usar os factories de `Error` (ex: `Error.NotFound` define `ErrorType.NotFound`).
**O que pode dar errado:** Usar `ErrorType.Failure` (genérico) para tudo, fazendo a API retornar 500 para erros que deveriam ser 400.
**Exemplo real de uso:**
```csharp
// Mapeamento automático no Controller Base:
return error.Type switch {
    ErrorType.NotFound => NotFound(...),
    ErrorType.Validation => BadRequest(...),
    _ => StatusCode(500, ...)
};
```

---

## `PagedRequest.cs`
**Responsabilidade:** Padronizar os parâmetros de entrada para endpoints e queries que suportam paginação e ordenação.
**Por que existe:** Para evitar duplicação de parâmetros (`page`, `size`, `sort`) em todos os DTOs de Query e garantir defaults seguros (ex: limitar PageSize a 100).
**Em que situação usar:** Como classe base ou propriedade composta em Queries de listagem.
**O que pode dar errado:** O usuário pedir `PageSize=1000000`. A classe já possui proteção no setter/init para limitar ao máximo de 100 itens por página.
**Exemplo real de uso:**
```csharp
// Na definição da Query no controller:
public record ListarProdutosQuery(string Categoria, PagedRequest Paginacao);

// Chamada na API:
// GET /produtos?categoria=eletronicos&pageNumber=2&pageSize=20
```

---

## `PaginatedList.cs`
**Responsabilidade:** Representar o resultado de uma consulta paginada, contendo os itens da página atual e metadados de navegação (Total de Páginas, Total de Itens).
**Por que existe:** Para encapsular a lógica de cálculo de páginas e fornecer um contrato único de resposta para listas na API.
**Em que situação usar:** Como tipo de retorno de Queries de listagem.
**O que pode dar errado:** Calcular o `TotalCount` incorretamente ou esquecer de aplicar o `Skip/Take` na consulta antes de passar os itens para o construtor (se não usar o Factory `CreateAsync` ou extensions).
**Exemplo real de uso:**
```csharp
var lista = await PaginatedList<Produto>.CreateAsync(query, 1, 10);
// Retorno JSON:
// { "items": [...], "pageNumber": 1, "totalPages": 5, "totalCount": 50 }
```

---

## `Result.cs`
**Responsabilidade:** Implementar o padrão **Result** (ou Monad), encapsulando o sucesso ou falha de uma operação e obrigando o tratamento explícito do erro.
**Por que existe:** Para eliminar a ambiguidade de métodos que retornam `null` em caso de erro e remover o uso de exceções para controle de fluxo de negócio.
**Em que situação usar:** Como tipo de retorno padrão de quase todos os Services, Handlers e métodos de Domínio.
**O que pode dar errado:** Acessar a propriedade `.Value` de um resultado sem verificar `.IsSuccess` antes. Isso lançará uma exceção `InvalidOperationException` propositalmente.
**Exemplo real de uso:**
```csharp
Result<Usuario> resultado = servico.CriarUsuario(dto);

if (resultado.IsFailure) {
    // Tratar erro
    Log(resultado.Error);
    return;
}

// Seguro para acessar Value agora
var usuario = resultado.Value;
```

---

## `SortDescriptor.cs`
**Responsabilidade:** Encapsular a intenção de ordenação dinâmica (campo e direção).
**Por que existe:** Para separar os dados de ordenação dos dados de filtro e permitir passagem fácil entre camadas.
**Em que situação usar:** Geralmente encapsulado automaticamente dentro de `PagedRequest`.
**O que pode dar errado:** Passar o nome da propriedade (`Field`) direto para uma query SQL bruta sem higienização, causando **SQL Injection**. Sempre use com LINQ/EF Core (que trata isso) ou valide contra uma lista de colunas permitidas.
**Exemplo real de uso:**
```csharp
var sort = new SortDescriptor("Preco", IsAscending: false);
// Aplicação no repositório:
query = query.OrderByDescending(x => x.Preco);
```
