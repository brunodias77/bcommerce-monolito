# Implementação do Cenário: Criação de Usuário e Carrinho (Sem RabbitMQ)

Neste cenário, operamos em um **Monolito Modular** onde todos os módulos (Users e Cart) rodam no mesmo processo (host), mas devem permanecer desacoplados.

Utilizaremos o padrão **Outbox** para garantir consistência (atomicidade) e um **Barramento em Memória (In-Memory Bus)** via `MediatR` para comunicação entre módulos.

## Fluxo de Execução

1.  **Users Module**: Recebe o comando de registro.
2.  **Users Module**: Cria o usuário no Identity e o Perfil no banco.
3.  **Users Module**: Gera um evento de integração `UserRegisteredIntegrationEvent`.
4.  **Infrastructure**: O `OutboxInterceptor` intercepta esse evento e o salva na tabela `shared.domain_events` na mesma transação do usuário.
5.  **Background Worker**: Um processo em segundo plano (dentro da mesma aplicação) lê a tabela de Outbox e publica o evento em memória.
6.  **Cart Module**: Possui um Handler que escuta esse evento e cria o carrinho.

---

## 1. Definição do Evento de Integração

No projeto `BuildingBlocks.Messaging` ou em um projeto `Shared.Contracts` (caso queira compartilhar contratos):

```csharp
// BuildingBlocks.Messaging/Abstractions/IIntegrationEvent.cs
public interface IIntegrationEvent : INotification // INotification do MediatR
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}

// Users.Application/IntegrationEvents/UserRegisteredIntegrationEvent.cs
public record UserRegisteredIntegrationEvent(Guid UserId, string Email, string UserName) : IIntegrationEvent;
```

## 2. Publicação no Módulo Users (Application)

No `RegisterUserCommandHandler`:

```csharp
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<UserProfile> _profileRepository;
    // O DbContext é injetado, mas a gestão da transação é feita pelo TransactionBehavior
    
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Criar Usuário no Identity
        var user = new ApplicationUser { UserName = command.Email, Email = command.Email };
        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded) return Result.Failure<Guid>(result.Errors.ToError());

        // 2. Criar Perfil (Entidade de Domínio)
        var profile = new UserProfile(user.Id, command.FirstName, command.LastName);
        
        // 3. Adicionar Evento de Domínio que será transformado em Evento de Integração
        // OU adicionar diretamente o evento de integração se a entidade suportar
        profile.AddDomainEvent(new UserRegisteredIntegrationEvent(user.Id, user.Email, user.UserName));

        // 4. Salvar no Repositório
        await _profileRepository.AddAsync(profile, cancellationToken);
        
        // O TransactionBehavior fará o Commit. 
        // O OutboxInterceptor interceptará o evento e salvará na tabela 'shared.domain_events'.
        
        return Result.Success(user.Id);
    }
}
```

## 3. Persistência com Outbox (Infrastructure)

O `OutboxInterceptor` configurado no `BuildingBlocks.Infrastructure` garante que o evento não seja perdido.

```csharp
// BuildingBlocks.Infrastructure/Persistence/Interceptors/OutboxInterceptor.cs
public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
{
    var context = eventData.Context;
    
    // Pega as entidades que têm eventos
    var aggregates = context.ChangeTracker.Entries<IAggregateRoot>()
        .Where(e => e.Entity.DomainEvents.Any())
        .Select(e => e.Entity);

    var outboxMessages = aggregates.SelectMany(a => a.DomainEvents)
        .Select(domainEvent => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOn = DateTime.UtcNow,
            Type = domainEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
        })
        .ToList();

    if (outboxMessages.Any())
    {
        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }

    return base.SavingChangesAsync(eventData, result, cancellationToken);
}
```

## 4. Processamento do Outbox (Sem RabbitMQ)

Criamos um `BackgroundService` que roda na aplicação e processa a tabela de Outbox, publicando via `MediatR` para outros módulos.

```csharp
// BuildingBlocks.Infrastructure/Processing/OutboxProcessor.cs
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>(); // MediatR

            // 1. Ler mensagens não processadas
            var messages = await dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedOn == null)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try 
                {
                    // 2. Deserializar o evento
                    var eventType = Type.GetType(message.Type); // Precisaria do Assembly Qualified Name
                    var integrationEvent = JsonConvert.DeserializeObject(message.Content, eventType) as INotification;

                    // 3. Publicar em memória para quem estiver ouvindo (Módulo Cart)
                    await publisher.Publish(integrationEvent, stoppingToken);

                    // 4. Marcar como processado
                    message.ProcessedOn = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.Error = ex.Message;
                }
            }
            
            await dbContext.SaveChangesAsync(stoppingToken);
            await Task.Delay(1000, stoppingToken); // Polling a cada 1s
        }
    }
}
```

## 5. Consumo no Módulo Cart (Application)

O módulo de Cart não conhece o módulo de Users, mas conhece o contrato do evento (que fica no Shared Kernel/Building Blocks ou projeto de Contratos).

```csharp
// Cart.Application/EventHandlers/UserRegisteredEventHandler.cs
public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredIntegrationEvent>
{
    private readonly ICartRepository _cartRepository;

    public UserRegisteredEventHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task Handle(UserRegisteredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Lógica para criar o carrinho
        var newCart = Cart.Create(notification.UserId);
        await _cartRepository.AddAsync(newCart, cancellationToken);
        
        // Salva as alterações (o TransactionBehavior do Cart Module cuida do commit)
    }
}
```

## Resumo da Arquitetura "Sem RabbitMQ"

1.  **Acoplamento**: Zero acoplamento de código direto. A comunicação é via `MediatR` + Evento Compartilhado.
2.  **Atomicidade**: Garantida pelo **Outbox Pattern**. Se falhar ao salvar o usuário, o evento não é salvo. Se falhar ao criar o carrinho, o `OutboxProcessor` tentará novamente (se implementarmos retry) ou marcará erro, mas o usuário já estará criado.
3.  **Transporte**: In-Process (Memória).
