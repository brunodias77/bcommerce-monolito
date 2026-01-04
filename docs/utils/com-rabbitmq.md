# Implementação do Cenário: Criação de Usuário e Carrinho (Com RabbitMQ)

Neste cenário, utilizamos um **Broker de Mensageria (RabbitMQ)** para comunicar os módulos. Isso permite que os módulos rodem em processos separados (Microserviços) ou no mesmo processo, mas com total isolamento e garantia de entrega.

Utilizaremos o **MassTransit** como biblioteca de abstração sobre o RabbitMQ e o padrão **Outbox** para garantir atomicidade na publicação.

## Fluxo de Execução

1.  **Users Module**: Recebe o comando de registro.
2.  **Users Module**: Cria o usuário e o perfil no banco.
3.  **Users Module**: Publica o evento via `IPublishEndpoint` (MassTransit).
4.  **Infrastructure**: O `Outbox` do MassTransit (configurado no EF Core) intercepta a mensagem e a salva na tabela de Outbox (não envia para o RabbitMQ imediatamente).
5.  **Relay Worker**: Um processo do MassTransit lê a tabela de Outbox e envia para o RabbitMQ (Exchange).
6.  **RabbitMQ**: Roteia a mensagem para a fila do módulo de Carrinho (`cart-service-queue`).
7.  **Cart Module**: Um Consumer (`IConsumer<T>`) processa a mensagem e cria o carrinho.

---

## 1. Configuração do MassTransit (Infrastructure)

No `Startup` ou `Program.cs` de cada módulo (ou no BuildingBlocks se for compartilhado):

```csharp
// BuildingBlocks.Messaging/Extensions/MassTransitExtensions.cs
services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<BaseDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox(); // Configura o Transactional Outbox
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost");
        cfg.ConfigureEndpoints(context);
    });
});
```

## 2. Publicação no Módulo Users (Application)

No `RegisterUserCommandHandler`, injetamos `IPublishEndpoint` do MassTransit.

```csharp
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<UserProfile> _profileRepository;
    private readonly IPublishEndpoint _publishEndpoint; // MassTransit

    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Criar Usuário no Identity
        var user = new ApplicationUser { UserName = command.Email, Email = command.Email };
        var result = await _userManager.CreateAsync(user, command.Password);
        
        // 2. Criar Perfil
        var profile = new UserProfile(user.Id, command.FirstName, command.LastName);
        await _profileRepository.AddAsync(profile, cancellationToken);
        
        // 3. Publicar Evento (Será capturado pelo Outbox do MassTransit)
        // O evento não vai para o RabbitMQ agora. Ele vai para a tabela 'OutboxMessage' do banco.
        await _publishEndpoint.Publish(new UserRegisteredIntegrationEvent(
            user.Id, 
            user.Email, 
            user.UserName
        ), cancellationToken);

        // 4. O TransactionBehavior fará o Commit do EF Core.
        // Isso commita: User, Profile e a Mensagem de Outbox na MESMA transação.
        
        return Result.Success(user.Id);
    }
}
```

## 3. Consumo no Módulo Cart (Application)

No módulo Cart, definimos um **Consumer** do MassTransit em vez de um `INotificationHandler`.

```csharp
// Cart.Application/Consumers/UserRegisteredConsumer.cs
public class UserRegisteredConsumer : IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly ICartRepository _cartRepository;
    // Opcional: ILogger, etc.

    public UserRegisteredConsumer(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var message = context.Message;
        
        // Idempotência: Verificar se já processamos essa mensagem (Inbox Pattern)
        // O MassTransit pode lidar com isso se configurarmos o Inbox, ou fazemos manual.
        
        // Lógica de Negócio
        var newCart = Cart.Create(message.UserId);
        
        await _cartRepository.AddAsync(newCart, context.CancellationToken);
        
        // Salva as alterações
        // Se der erro aqui, o MassTransit fará retry e depois moverá para DLQ (Dead Letter Queue)
    }
}
```

## 4. Registro do Consumer (Cart Module)

No setup do módulo Cart:

```csharp
services.AddMassTransit(x =>
{
    // Registra o Consumer
    x.AddConsumer<UserRegisteredConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost");
        
        // Configura a fila específica do Cart
        cfg.ReceiveEndpoint("cart-service", e =>
        {
            e.ConfigureConsumer<UserRegisteredConsumer>(context);
        });
    });
});
```

## Resumo da Arquitetura "Com RabbitMQ"

1.  **Acoplamento**: Zero. Comunicação via Contrato de Mensagem.
2.  **Atomicidade**: Garantida pelo **Transactional Outbox** nativo do MassTransit + EF Core.
3.  **Resiliência**:
    *   Se o RabbitMQ cair, a aplicação continua funcionando (mensagem fica no banco).
    *   Se o Consumer do Cart falhar, o MassTransit faz retries automáticos.
    *   Se falhar definitivamente, vai para uma fila de erro (DLQ) para análise.
4.  **Escalabilidade**: O módulo Cart pode ter 10 instâncias consumindo a fila para processar milhares de criações de usuário em paralelo.
