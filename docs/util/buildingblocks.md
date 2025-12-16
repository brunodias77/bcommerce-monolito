Aqui está uma versão aprimorada e mais detalhada da documentação, rica em exemplos práticos e explicações arquiteturais baseadas no seu código.

---

#🏗️ Building Blocks: Guia de Padrões ArquiteturaisEste documento detalha os padrões arquiteturais implementados na biblioteca central `BuildingBlocks`. Ele serve como guia para entender **quais problemas resolvemos**, **como implementamos** e **onde configurar** cada padrão dentro da arquitetura do BCommerce.

---

##📋 Índice1. [Outbox Pattern (Eventos Confiáveis)](https://www.google.com/search?q=%231-outbox-pattern)
2. [CQRS (Command Query Responsibility Segregation)](https://www.google.com/search?q=%232-cqrs)
3. [MediatR Pipeline Behaviors](https://www.google.com/search?q=%233-pipeline-behaviors)
4. [Result Pattern (Tratamento de Erros)](https://www.google.com/search?q=%234-result-pattern)
5. [Domain Events (Eventos de Domínio)](https://www.google.com/search?q=%235-domain-events)
6. [EF Core Interceptors (Auditoria e Soft Delete)](https://www.google.com/search?q=%236-ef-core-interceptors)
7. [Value Objects (Objetos de Valor)](https://www.google.com/search?q=%237-value-objects)
8. [Smart Enums (Enumerações Ricas)](https://www.google.com/search?q=%238-smart-enums)
9. [Global Exception Handling](https://www.google.com/search?q=%239-global-exception-handling)

---

##1. Outbox Pattern###🤕 A Dor (Problema)O clássico **"Dual Write Problem"**.
Exemplo: Um usuário se cadastra. Você precisa:

1. Salvar o usuário no banco de dados (`users.asp_net_users`).
2. Publicar um evento para o módulo de Carrinho (`UserCreatedIntegrationEvent`).

Se o banco comitar e o broker de mensagens falhar (ou o processo cair), o usuário é criado mas o carrinho nunca será gerado. O sistema fica inconsistente.

###✅ A SoluçãoSalvar o evento **na mesma transação** do banco de dados, em uma tabela auxiliar (`shared.domain_events`). Um processo em background lê essa tabela e despacha as mensagens com garantia de entrega.

###⚙️ Como Configurar e Usar**1. No Core (`Modules.Users.Core`)**
Defina o evento de integração.

```csharp
// Em Users.Contracts/Events/UserCreatedIntegrationEvent.cs
public record UserCreatedIntegrationEvent(Guid UserId, string Email) : IIntegrationEvent;

```

**2. Na Infrastructure (`Modules.Users.Infrastructure`)**
No `UsersDbContext`, registre o `PublishDomainEventsInterceptor`. Ele intercepta o `SaveChanges`, captura os eventos e salva na tabela de Outbox.

```csharp
// Em Users.Infrastructure/Persistence/UsersDbContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // O interceptor "users" é injetado via DI no Startup
    optionsBuilder.AddInterceptors(_publishDomainEventsInterceptor); 
}

```

**3. Na API (`Bcommerce.Api/Configurations/InfraDependencyInjection.cs`)**
Registre o interceptor (com chave para identificar o módulo) e o Background Job.

```csharp
// Registra uma instância específica para o módulo 'users'
services.AddKeyedSingleton("users", (sp, key) => new PublishDomainEventsInterceptor("users"));

// Job que lê a tabela shared.domain_events e processa
services.AddOutboxProcessor(options => {
    options.ProcessInterval = TimeSpan.FromSeconds(2); // Polling a cada 2s
    options.BatchSize = 20;
});

```

---

##2. CQRS###🤕 A Dor (Problema)Usar o mesmo modelo de objeto para leitura e escrita cria conflitos.

* **Escrita:** Precisa de validações complexas, regras de negócio e transações.
* **Leitura:** Precisa de performance, joins específicos e dados "mastigados" para a tela.
* Tentar usar a mesma Entidade do EF Core para ambos resulta em queries lentas ou entidades anêmicas.

###✅ A SoluçãoSeparar explicitamente:

* **Commands (Escrita):** Intenção de mudar estado. Retornam apenas sucesso/falha ou IDs.
* **Queries (Leitura):** Intenção de buscar dados. Retornam DTOs otimizados. Não mudam estado.

###⚙️ Como Configurar e Usar**1. Na Application (`Modules.Users.Application`)**
Implemente as interfaces `ICommand` e `IQuery` do BuildingBlocks.

```csharp
// --- ESCRITA ---
// Define o comando e o retorno (Guid)
public record CreateUserCommand(string Email, string Name) : ICommand<Guid>;

// Handler contém a regra de negócio
public class CreateUserHandler : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand cmd, CancellationToken ct) { ... }
}

// --- LEITURA ---
// Define a query e o DTO de retorno
public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

// Handler otimizado para leitura (pode usar Dapper ou EF AsNoTracking)
public class GetUserQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken ct) { ... }
}

```

**2. Na API (`Bcommerce.Api/Configurations/ApplicationDependencyInjection.cs`)**
O MediatR escaneia os assemblies para conectar Commands aos Handlers.

```csharp
services.AddMediatR(cfg => {
    // Registra todos os handlers do módulo Users
    cfg.RegisterServicesFromAssembly(typeof(Users.Application.AssemblyReference).Assembly);
});

```

---

##3. Pipeline Behaviors###🤕 A Dor (Problema)Código "boilerplate" repetido em todo Handler:

* `_logger.LogInformation("Iniciando...")`
* `if (!validator.Validate()) return Error`
* `try { await _unitOfWork.SaveChanges() } catch { ... }`

Isso polui a regra de negócio e viola o DRY (Don't Repeat Yourself).

###✅ A SoluçãoUsar o padrão **Decorator** (via MediatR Pipeline) para envolver os Handlers com comportamentos transversais.

###⚙️ Como Configurar e Usar**1. Na API (`Bcommerce.Api/Configurations/ApplicationDependencyInjection.cs`)**
A ordem de registro define a ordem de execução ("cebola").

```csharp
// 1. Logging (Mais externo): Loga entrada e saída de TODOS os requests.
services.AddLoggingBehavior();

// 2. Validation: Executa FluentValidation. Se falhar, nem chama o Handler.
services.AddValidationBehavior();

// 3. Transaction (Mais interno): Abre transação APENAS para Commands.
//    Queries passam direto por esse behavior.
services.AddTransactionBehavior();

```

**2. Na Application (`Modules.Users.Application`)**
Basta criar um validador. O `ValidationBehavior` o encontrará e executará automaticamente.

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator() 
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");
    }
}

```

---

##4. Result Pattern###🤕 A Dor (Problema)Uso excessivo de Exceptions para controle de fluxo (`UserNotFoundException`, `InsufficientFundsException`).

* **Performance:** Exceptions são caras em .NET.
* **Legibilidade:** Não fica claro na assinatura do método que ele pode falhar. É um "GOTO" invisível.

###✅ A SoluçãoUm objeto `Result` ou `Result<T>` que encapsula o sucesso ou a falha. O fluxo é explícito e funcional.

###⚙️ Como Configurar e Usar**1. No Domain/Application**
Retorne `Result` em vez de lançar exceções de negócio.

```csharp
public Result<Product> ReduceStock(int quantity)
{
    if (this.Stock < quantity)
    {
        // Retorna um erro tipado (Conflict - 409)
        return Result.Fail<Product>(Error.Conflict("STOCK_LOW", "Estoque insuficiente"));
    }
    
    this.Stock -= quantity;
    return Result.Ok(this);
}

```

**2. Na Presentation (`Modules.Users.Presentation/Controllers`)**
Use a classe base `ApiControllerBase` para converter o `Result` em resposta HTTP padrão (RFC 7807).

```csharp
public class UsersController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
    {
        Result<Guid> result = await Mediator.Send(command);
        
        // Se Sucesso: Retorna 201 Created com o ID
        // Se Falha (Validation): Retorna 400 Bad Request
        // Se Falha (Conflict): Retorna 409 Conflict
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Value }); 
    }
}

```

---

##5. Domain Events###🤕 A Dor (Problema)Regras de negócio acopladas.
Exemplo: O método `Order.Confirmar()` não deveria saber como enviar e-mail, dar baixa no estoque e notificar o usuário. Se colocar tudo ali, a classe `Order` vira um "God Object".

###✅ A SoluçãoInversão de controle. A entidade apenas diz **"Ocorreu X"** (passado). Handlers externos reagem a isso.

###⚙️ Como Configurar e Usar**1. No Core (`Modules.Catalog.Core`)**
A entidade registra o evento em sua lista interna (`_domainEvents`).

```csharp
public class Product : Entity 
{
    public void UpdatePrice(decimal newPrice) 
    {
        var oldPrice = this.Price;
        this.Price = newPrice;
        
        // Apenas adiciona à memória. NÃO dispara ainda.
        this.AddDomainEvent(new ProductPriceChangedEvent(this.Id, oldPrice, newPrice));
    }
}

```

**2. Na Infrastructure**
O `PublishDomainEventsInterceptor` detecta que a entidade tem eventos pendentes e, antes de salvar no banco, converte esses eventos para o Outbox. Isso garante atomicidade.

---

##6. EF Core Interceptors###🤕 A Dor (Problema)* Esquecer de setar `UpdatedAt = DateTime.Now` em todo update.
* Implementar "Soft Delete" manualmente com `WHERE DeletedAt IS NULL` em todas as queries.

###✅ A SoluçãoInterceptar o comando `SaveChanges` do EF Core e injetar lógica globalmente.

###⚙️ Como Configurar e Usar**1. No Core (`Modules.Users.Core`)**
Marque suas entidades com interfaces.

```csharp
public class User : Entity, IAuditableEntity, ISoftDeletable
{
    // Propriedades exigidas pelas interfaces
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}

```

**2. Na Infrastructure (`Modules.Users.Infrastructure`)**
Configure o DbContext.

```csharp
// UsersDbContext.cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // Filtro Global: O EF Core adiciona "AND DeletedAt IS NULL" em TODAS as queries automaticamente
    builder.Entity<User>().HasQueryFilter(x => x.DeletedAt == null);
}

// Configuração no DependencyInjection.cs
services.AddDbContext<UsersDbContext>((sp, options) => {
    options.AddInterceptors(
        sp.GetRequiredService<AuditableEntityInterceptor>(), // Preenche datas
        sp.GetRequiredService<SoftDeleteInterceptor>()       // Muda State=Deleted para State=Modified
    );
});

```

---

##7. Value Objects###🤕 A Dor (Problema)"Primitive Obsession". Usar `string` para CPF, Email, CEP.

* Risco de passar dados inválidos.
* Lógica de validação espalhada (repetir Regex de email em 10 lugares).
* Confusão de parâmetros (`void Metodo(string email, string nome)` vs `void Metodo(string nome, string email)`).

###✅ A SoluçãoClasses imutáveis que encapsulam a validação. Se o objeto existe, ele é válido.

###⚙️ Como Configurar e Usar**1. No Core (`BuildingBlocks.Domain/Models`)**
Herde de `ValueObject`.

```csharp
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) { Value = value; }

    public static Result<Email> Create(string email)
    {
        if (!Regex.IsMatch(email, "...")) 
            return Result.Fail<Email>("Email inválido");
            
        return Result.Ok(new Email(email));
    }

    // Garante igualdade por valor (email1 == email2 se os textos forem iguais)
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

```

**2. No Uso**

```csharp
// O compilador impede passar uma string qualquer. Exige um objeto Email válido.
public void UpdateEmail(Email newEmail) { ... }

```

---

##8. Smart Enums###🤕 A Dor (Problema)Enums do C# são apenas `int`.

* Não podem ter comportamento (`OrderStatus.Paid.CanBeCancelled()`).
* Não suportam herança ou lógica complexa.

###✅ A SoluçãoClasses que parecem Enums, mas são objetos completos.

###⚙️ Como Configurar e Usar**1. No Core**
Herde de `Enumeration`.

```csharp
public class OrderStatus : Enumeration
{
    public static OrderStatus Paid = new(1, "Paid");
    public static OrderStatus Shipped = new(2, "Shipped");

    public OrderStatus(int id, string name) : base(id, name) { }

    // Lógica encapsulada!
    public bool CanCancel => this == Paid; 
}

```

---

##9. Global Exception Handling###🤕 A Dor (Problema)Blocos `try/catch` em todos os Controllers. Se um erro não tratado ocorrer, a API retorna stack trace (inseguro) ou erro 500 genérico sem detalhes.

###✅ A SoluçãoMiddleware centralizado que captura qualquer exceção não tratada e a converte em uma resposta JSON padronizada (ProblemDetails).

###⚙️ Como Configurar e Usar**1. Na Presentation (`BuildingBlocks.Presentation/Middleware`)**
O `ExceptionHandlingMiddleware` mapeia exceções para Status Codes.

```csharp
private static (int StatusCode, string Title) MapException(Exception ex) => ex switch
{
    EntityNotFoundException => (404, "Não Encontrado"),
    BusinessRuleValidationException => (409, "Erro de Negócio"),
    _ => (500, "Erro Interno")
};

```

**2. Na API (`Program.cs`)**
Deve ser um dos primeiros middlewares.

```csharp
var app = builder.Build();
app.UseExceptionHandlingMiddleware(); // Captura tudo que vem depois
app.MapControllers();

```