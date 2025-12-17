# Abstrações da Camada de Aplicação

Este diretório contém os contratos fundamentais (Interfaces) que regem o comportamento da camada de aplicação do sistema. Ele serve como o núcleo de inversão de dependência, permitindo que o "Core" da aplicação defina **o que** precisa ser feito, sem saber **como** será implementado.

## 🎯 Para que serve?

Estas abstrações definem a "linguagem" que os casos de uso (Handlers) utilizam para interagir com o mundo externo (Banco de Dados, Filas, APIs de Terceiros, Data/Hora). Elas isolam a regra de negócio da infraestrutura concreta.

## 💊 Qual dor isso resolve?

1.  **Acoplamento Forte**: Evita que sua regra de negócio dependa de bibliotecas pesadas (como Entity Framework, MassTransit, SmtpClient).
2.  **Testabilidade Difícil**: Permite criar "Mocks" ou "Stubs" facilmente. Você pode testar se um pedido foi criado sem precisar de um banco de dados real ou sem enviar um email de verdade.
3.  **Manutenibilidade**: Se você quiser trocar o ORM (ex: de EF Core para Dapper) ou o provedor de Email (ex: de AWS SES para SendGrid), basta criar uma nova implementação desta interface, sem tocar em nenhuma linha da regra de negócio.

---

## 📂 Data (Dados)

Interfaces para persistência e recuperação de dados.

### `IRepository<T>`
**O que é:** Contrato para repositórios de escrita focados em raízes de agregação (Aggregate Roots).
**Dor que resolve:** Garante que toda alteração de estado passe pelas regras de negócio da Entidade/Agregado, evitando manipulação direta de tabelas anêmicas.

```csharp
// Exemplo real no Handler
public class CriarPedidoHandler(IRepository<Pedido> pedidoRepo)
{
    public async Task Handle(...)
    {
        var pedido = new Pedido(...);
        // O handler não sabe se está salvando no SQL Server ou MongoDb
        await pedidoRepo.AddAsync(pedido); 
    }
}
```

### `IReadRepository<T>`
**O que é:** Contrato segregado apenas para leitura de dados (Interface Segregation Principle).
**Dor que resolve:** Evita expor métodos de alteração (`Add`, `Update`, `Delete`) em cenários onde apenas a leitura é necessária, prevenindo efeitos colaterais acidentais em Queries.

```csharp
// Exemplo real em um Validador ou Query
public class ValidarEstoqueServices(IReadRepository<Produto> produtoRepo)
{
    public async Task Validar(Guid produtoId)
    {
        // Acesso leve, apenas leitura
        var produto = await produtoRepo.GetByIdAsync(produtoId);
    }
}
```

### `IUnitOfWork`
**O que é:** Abstração para transações atômicas de banco de dados.
**Dor que resolve:** Inconsistência de dados. Garante que, se você alterar 3 tabelas diferentes num mesmo fluxo, ou tudo é salvo (`Commit`), ou nada é salvo (`Rollback`), mantendo o banco íntegro.

```csharp
// Exemplo real
await _pedidoRepo.AddAsync(pedido);
await _estoqueRepo.UpdateAsync(estoque);

// Só aqui as mudanças realmente vão para o banco (Transação DB)
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

---

## 📨 Messaging (Mensageria e CQRS)

Interfaces para implementação do padrão CQRS (Command Query Responsibility Segregation) e mensageria.

### `ICommand` e `ICommand<T>`
**O que é:** Representa uma intenção de **alterar** o estado do sistema (Escrever/Write).
**Dor que resolve:** Organização e clareza. Separa explicitamente operações que mudam dados daquelas que apenas leem.

```csharp
// Comando sem retorno (apenas sucesso/falha)
public record DeletarCategoriaCommand(Guid Id) : ICommand;

// Comando com retorno (ex: ID gerado)
public record CriarClienteCommand(string Nome, string Email) : ICommand<Guid>;
```

### `IQuery<T>`
**O que é:** Representa uma intenção de **ler** dados do sistema.
**Dor que resolve:** Performance e clareza. Permite otimizar o lado da leitura (ex: usar SQL puro/Dapper) sem se preocupar com as regras complexas de validação do lado da escrita.

```csharp
// Query para obter detalhes de um produto
public record ObterProdutoPorIdQuery(Guid Id) : IQuery<ProdutoDto>;
```

### `ICommandHandler` e `IQueryHandler`
**O que é:** O local onde a mágica acontece. A classe que recebe o Command/Query e executa a lógica.
**Dor que resolve:** Fat Controllers. Tira a lógica de negócio dos Controllers da API, deixando-os magros e focados apenas em HTTP.

```csharp
public class CriarClienteHandler : ICommandHandler<CriarClienteCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken ct) { ... }
}
```

### `IIntegrationEvent`
**O que é:** Uma mensagem que notifica outros sistemas de que algo aconteceu (`Past Tense`). Ex: `PedidoCriado`, `PagamentoAprovado`.
**Dor que resolve:** Acoplamento temporal e entre serviços. Permite que o serviço de "Estoque" saiba que um pedido foi criado sem que o serviço de "Pedidos" precise chamar o "Estoque" diretamente (síncrono).

```csharp
// Evento disparado após o pedido ser salvo com sucesso
public record PedidoCriadoEvent(Guid PedidoId, decimal ValorTotal) : IIntegrationEvent;
```

### `IIntegrationEventHandler`
**O que é:** O consumidor que reage ao evento de integração.
**Dor que resolve:** Permite reações assíncronas e desacopladas. O sistema pode continuar funcionando mesmo se o consumidor estiver fora do ar (graças à fila).

```csharp
public class EnviarEmailConfirmacaoHandler : IIntegrationEventHandler<PedidoCriadoEvent>
{
    // Reage ao evento enviando email, sem travar o fluxo original do pedido
    public async Task Handle(PedidoCriadoEvent @event, CancellationToken ct) { ... }
}
```

---

## 🛠 Services (Serviços Utilitários)

Abstrações para recursos transversais de infraestrutura.

### `IDateTimeProvider`
**O que é:** Fornecedor de data e hora.
**Dor que resolve:** Testes Unitários de lógicas temporais. Como testar uma regra "se for sexta-feira, dê desconto" numa terça-feira? Com `DateTime.Now` é impossível. Com essa interface, você "finge" ser sexta-feira nos testes.

```csharp
// Errado (na regra de negócio):
if (DateTime.Now.DayOfWeek == DayOfWeek.Friday) ...

// Certo:
if (_dateTimeProvider.UtcNow.DayOfWeek == DayOfWeek.Friday) ...
```

### `ICurrentUserService`
**O que é:** Acesso aos dados do usuário logado (Contexto).
**Dor que resolve:** Dependência de HTTP. Sua regra de negócio não deveria conhecer `HttpContext`. Essa interface permite obter o ID do usuário logado de forma limpa, funcionando até mesmo se a chamada vier de um Job ou Fila (onde não existe HttpContext).

```csharp
var usuarioId = _currentUserService.UserId;
// Regra: O usuário só pode alterar o próprio perfil
if (usuarioId != perfil.DonoId) throw new UnauthorizedException(...);
```

### `IEmailService`
**O que é:** Contrato para envio de emails.
**Dor que resolve:** Vendor Lock-in. Se hoje usamos SMTP e amanhã quisermos mudar para SendGrid API ou Amazon SES, mudamos apenas a implementação dessa interface, e o resto do sistema nem percebe.

```csharp
await _emailService.SendEmailAsync(
    to: cliente.Email, 
    subject: "Bem-vindo!", 
    body: "<h1>Olá...</h1>"
);
```
