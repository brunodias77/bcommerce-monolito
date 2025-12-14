Aqui está um guia detalhado em formato Markdown explicando os padrões implementados no projeto `BuildingBlocks`, as dores que eles resolvem e como configurá-los em sua aplicação.

---

#🏗️ Building Blocks: Padrões e Guia de ConfiguraçãoEste documento detalha os padrões arquiteturais implementados na biblioteca compartilhada `BuildingBlocks`, explicando **qual problema cada um resolve** e **como configurá-los** nas camadas apropriadas do seu Monolito Modular.

---

##1. Outbox Pattern (Eventos Confiáveis)###🤕 A Dor (Problema)O "Dual Write Problem": Quando você precisa salvar uma alteração no banco de dados e publicar uma mensagem (evento) para outro módulo/sistema. Se o banco comitar mas o broker de mensagem falhar (ou vice-versa), seu sistema fica inconsistente e o evento é perdido.

###✅ A SoluçãoSalvar o evento na mesma transação do banco de dados em uma tabela `outbox`. Um processo em background lê essa tabela posteriormente e envia as mensagens com garantia de entrega.

###⚙️ Como Configurar**1. Infrastructure (`Modules.{Module}.Infrastructure`)**
No `DbContext` do módulo, registre o interceptor que captura eventos e os salva no Outbox.

```csharp
// Em Modules/Users/Users.Infrastructure/Persistence/UsersDbContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // O interceptor é injetado via Dependency Injection no Startup
    optionsBuilder.AddInterceptors(_publishDomainEventsInterceptor); 
}

```

**2. API (`Bcommerce.Api/Program.cs`)**
Registre o interceptor e o Background Job que processa as mensagens.

```csharp
// Registra uma instância do interceptor para cada módulo (identificado por chave)
builder.Services.AddKeyedSingleton("users", (sp, key) => new PublishDomainEventsInterceptor("users"));

// Registra o Job que processa a tabela shared.domain_events
builder.Services.AddHostedService<ProcessOutboxMessagesJob>();

```

---

##2. CQRS (Command Query Responsibility Segregation)###🤕 A Dor (Problema)Usar o mesmo modelo de objeto para leitura e escrita torna o sistema complexo e lento. Validações complexas de escrita atrapalham consultas simples, e consultas performáticas exigem "burlar" as regras de domínio.

###✅ A SoluçãoSeparar as operações em **Commands** (mudam estado, não retornam dados complexos) e **Queries** (retornam dados, não mudam estado).

###⚙️ Como Configurar**1. Application (`Modules.{Module}.Application`)**
Defina Commands e Queries implementando as interfaces do BuildingBlocks.

```csharp
// Command (Escrita)
public record CreateUserCommand(string Email) : ICommand<Guid>;

// Query (Leitura)
public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

```

**2. API (`Bcommerce.Api/Program.cs`)**
Registre o MediatR para escanear os handlers dos seus módulos.

```csharp
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Users.Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Catalog.Application.AssemblyReference).Assembly);
});

```

---

##3. Pipeline Behaviors (Cross-Cutting Concerns)###🤕 A Dor (Problema)Código repetitivo em todo Handler: "Logar entrada...", "Validar dados...", "Abrir transação...", "Try/Catch...", "Logar erro...". Isso suja a regra de negócio.

###✅ A SoluçãoUsar Behaviors do MediatR para interceptar a requisição antes e depois do Handler, centralizando essa lógica.

###⚙️ Como Configurar**1. API (`Bcommerce.Api/Program.cs`)**
A ordem de registro define a ordem de execução (cebola).

```csharp
// 1. Logging (Mais externo: loga tudo, inclusive erros de validação)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// 2. Validation (Valida antes de abrir transação)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// 3. Transaction (Mais interno: abre transação apenas para Commands válidos)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

```

**2. Application (`Modules.{Module}.Application`)**
Crie validadores usando FluentValidation. O `ValidationBehavior` irá encontrá-los automaticamente.

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator() {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

```

---

##4. Result Pattern###🤕 A Dor (Problema)Usar `Exceptions` para controle de fluxo (ex: `UserNotFoundException` para um fluxo normal de busca). Isso é caro (performance) e torna o fluxo do código confuso (GOTO disfarçado).

###✅ A SoluçãoRetornar um objeto `Result` que indica explicitamente Sucesso ou Falha, contendo o valor ou o erro, forçando quem chama a tratar o resultado.

###⚙️ Como Configurar**1. Application (`Modules.{Module}.Application`)**
Retorne `Result` nos seus Handlers.

```csharp
public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
{
    if (await _repo.Exists(command.Email))
    {
        // Retorna erro de negócio sem lançar exceção
        return Result.Fail<Guid>(UserErrors.EmailAlreadyExists);
    }
    
    // ... lógica ...
    return Result.Ok(user.Id);
}

```

**2. Presentation (`Modules.{Module}.Presentation` / Controllers)**
Use a classe base `ApiControllerBase` para converter `Result` em `IActionResult`.

```csharp
public class UsersController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        
        // Converte automaticamente:
        // Success -> 200 OK / 201 Created
        // Failure (Validation) -> 400 Bad Request
        // Failure (NotFound) -> 404 Not Found
        return HandleResult(result); 
    }
}

```

---

##5. Domain Events (Eventos de Domínio)###🤕 A Dor (Problema)Acoplamento alto entre regras de negócio. Exemplo: Ao criar um pedido, atualizar estoque, limpar carrinho e enviar e-mail, tudo no mesmo método gigante.

###✅ A SoluçãoA entidade dispara um evento "Ocorreu X". Handlers específicos reagem a isso. O código principal foca apenas em "fazer X".

###⚙️ Como Configurar**1. Core (`Modules.{Module}.Core`)**
A entidade adiciona o evento à sua lista interna.

```csharp
public class User : Entity 
{
    public static User Create(...) 
    {
        var user = new User(...);
        // Apenas registra na memória, não publica ainda
        user.AddDomainEvent(new UserCreatedEvent(user.Id));
        return user;
    }
}

```

**2. Infrastructure (`Modules.{Module}.Infrastructure`)**
O `PublishDomainEventsInterceptor` (mencionado no item 1 - Outbox) irá interceptar o `SaveChangesAsync`, pegar esses eventos da memória e salvá-los no Outbox automaticamente.

---

##6. Smart Enums (Enumeration)###🤕 A Dor (Problema)"Primitive Obsession". Usar `int` ou `string` para status (ex: `status == 1`). Falta de lugar para colocar lógica associada ao status (ex: "Posso cancelar se o status for 1 ou 2?").

###✅ A SoluçãoClasses que se comportam como Enums, mas permitem métodos e validações.

###⚙️ Como Configurar**1. Core (`Modules.{Module}.Core`)**

```csharp
public class OrderStatus : Enumeration
{
    public static OrderStatus Pending = new(1, "Pending");
    public static OrderStatus Paid = new(2, "Paid");
    public static OrderStatus Shipped = new(3, "Shipped");

    public OrderStatus(int id, string name) : base(id, name) { }

    // Lógica encapsulada no próprio enum!
    public bool CanBeCancelled => this == Pending || this == Paid;
}

```

---

##7. EF Core Interceptors (Auditoria e Soft Delete)###🤕 A Dor (Problema)Esquecer de preencher `CreatedAt` ou `UpdatedAt`. Deletar dados fisicamente e perder histórico (`DELETE FROM`).

###✅ A SoluçãoInterceptors que alteram o comportamento do `SaveChanges` globalmente.

###⚙️ Como Configurar**1. Core (`Modules.{Module}.Core`)**
Implemente as interfaces marcadoras nas suas entidades.

```csharp
public class Product : Entity, IAuditableEntity, ISoftDeletable
{
    // ... propriedades
}

```

**2. Infrastructure (`Modules.{Module}.Infrastructure`)**
No `DbContext` do módulo, configure o filtro global para Soft Delete e registre os interceptors.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Filtro global: nunca traga deletados nas queries
    builder.Entity<Product>().HasQueryFilter(x => x.DeletedAt == null);
}

protected override void OnConfiguring(DbContextOptionsBuilder options)
{
    // Injetam comportamento automático
    options.AddInterceptors(
        _auditableEntityInterceptor, // Preenche CreatedAt/UpdatedAt
        _softDeleteInterceptor       // Transforma gb.Remove(x) em UPDATE x SET DeletedAt = Now
    );
}

```

---

##8. Value Objects###🤕 A Dor (Problema)Repetir validação de formato (CPF, Email, CEP) em todo lugar. Risco de trocar parâmetros (passar string de Email onde espera string de Nome).

###✅ A SoluçãoObjetos imutáveis validados na construção. Se o objeto existe, ele é válido. Igualdade por valor, não por referência.

###⚙️ Como Configurar**1. Core (`Modules.{Module}.Core`)**
Use as classes base do BuildingBlocks ou crie novas.

```csharp
// Em vez de:
// public void Register(string email) { ... valida regex ... }

// Use:
public void Register(Email email) 
{ 
    // Se chegou aqui, 'email' é garantidamente válido.
    // Lógica de validação fica isolada na classe Email.
}

```