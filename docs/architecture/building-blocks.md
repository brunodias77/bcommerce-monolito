src/buildingblocks/
│
├── BuildingBlocks.Domain/                     # O Coração (Puro C#)
│   ├── Entities/
│   │   ├── Entity.cs                          # Base com Id e DomainEvents
│   │   ├── AggregateRoot.cs                   # Base com Version (Optimistic Concurrency)
│   │   ├── IAuditableEntity.cs                # Para created_at, updated_at
│   │   └── ISoftDeletable.cs                  # Para deleted_at (seu schema usa muito)
│   │
│   ├── Events/
│   │   ├── IDomainEvent.cs                    # Interface marcadora
│   │   └── DomainEvent.cs                     # Base com OccurredOn
│   │
│   ├── Models/                                # Substitui ValueObjects complexos
│   │   ├── ValueObject.cs                     # Base para igualdade
│   │   └── Enumeration.cs                     # Para seus Enums (OrderStatus, etc)
│   │
│   ├── Repositories/
│   │   ├── IRepository.cs                     # Interface marcadora (Unit of Work implícito no EF)
│   │   └── IUnitOfWork.cs                     # Apenas SaveChangesAsync
│   │
│   └── Exceptions/
│       └── DomainException.cs                 # Base para erros de regra de negócio
│
├── BuildingBlocks.Application/                # Casos de Uso (CQRS com MediatR)
│   ├── Abstractions/
│   │   ├── ICommand.cs                        # IRequest<Result>
│   │   ├── IQuery.cs                          # IRequest<Result<T>>
│   │   └── ICommandHandler.cs
│   │
│   ├── Behaviors/                             # Pipelines do MediatR (Crucial)
│   │   ├── ValidationBehavior.cs              # FluentValidation automático
│   │   ├── LoggingBehavior.cs                 # Logs de entrada/saída
│   │   └── TransactionBehavior.cs             # Abre transação no Command
│   │
│   ├── Pagination/
│   │   ├── PagedResult.cs                     # Retorno padrão de listas
│   │   └── PaginationParams.cs
│   │
│   └── Results/                               # Pattern Result (Evita exceptions para fluxo)
│       ├── Result.cs
│       ├── Error.cs
│       └── ResultT.cs
│
└── BuildingBlocks.Infrastructure/             # A "Mágica" do EF Core e Outbox
    ├── Persistence/
    │   ├── SharedDbContext.cs                 # DbContext base (se for compartilhar conexões)
    │   ├── Configurations/                    # Configurações globais do EF
    │   │   └── BaseEntityConfiguration.cs     # Mapeia Id, CreatedAt automáticos
    │   │
    │   └── Interceptors/                      # LIGAÇÃO DIRETA COM SEU SCHEMA
    │       ├── AuditableEntityInterceptor.cs  # Preenche created_at/updated_at
    │       ├── SoftDeleteInterceptor.cs       # Intercepta Delete e faz update no deleted_at
    │       ├── OptimisticConcurrencyInterceptor.cs # Incrementa version (+1)
    │       └── PublishDomainEventsInterceptor.cs   # Pega eventos e joga no Outbox
    │
    ├── Messaging/                             # Outbox/Inbox Simplificado
    │   ├── Outbox/
    │   │   ├── OutboxMessage.cs               # Mapeia para shared.domain_events
    │   │   └── OutboxInterceptor.cs           # Salva eventos na tabela durante o SaveChanges
    │   │
    │   └── Integration/                       # Se usar RabbitMQ no futuro
    │       └── IEventBus.cs
    │
    └── Services/
        └── DateTimeProvider.cs                # Para testabilidade de datas