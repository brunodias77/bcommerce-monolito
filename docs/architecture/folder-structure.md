# Estrutura de Pastas – E-commerce Modular Monolith

## Visão Geral

Esta estrutura segue os princípios de **Clean Architecture**, **DDD (Domain-Driven Design)** e **Modular Monolith**, organizando o código em camadas bem definidas e módulos independentes.

---

## Estrutura Completa

```text
Bcommerce-Monolito/
│
├── src/
│   ├── api/
│   │   └── Bcommerce.Api/                          # API Layer - ASP.NET Core Web API
│   │       ├── Controllers/                        # Controllers REST por módulo
│   │       ├── Configurations/                     # Configurações de startup
│   │       ├── Extensions/                         # Extension methods
│   │       ├── Filters/                            # Action/Exception filters
│   │       ├── Middlewares/                        # Custom middlewares
│   │       ├── HealthChecks/                       # Health checks endpoints
│   │       ├── Models/                             # Request/Response models
│   │       └── Properties/                         # Assembly properties
│
├── BuildingBlocks.Domain/                          # O Coração (Puro C#)
│   ├── Entities/
│   │   ├── Entity.cs                               # Base com Id e DomainEvents
│   │   ├── AggregateRoot.cs                        # Base com Version (Optimistic Concurrency)
│   │   ├── IAuditableEntity.cs                     # created_at, updated_at
│   │   └── ISoftDeletable.cs                       # deleted_at
│   ├── Events/
│   │   ├── IDomainEvent.cs
│   │   └── DomainEvent.cs                          # OccurredOn
│   ├── Models/
│   │   ├── ValueObject.cs                          # Base para igualdade
│   │   └── Enumeration.cs                          # Enums ricos
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs                          # SaveChangesAsync
│   └── Exceptions/
│       └── DomainException.cs
│
├── BuildingBlocks.Application/                     # Casos de Uso (CQRS + MediatR)
│   ├── Abstractions/
│   │   ├── ICommand.cs
│   │   ├── IQuery.cs
│   │   └── ICommandHandler.cs
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── TransactionBehavior.cs
│   ├── Pagination/
│   │   ├── PagedResult.cs
│   │   └── PaginationParams.cs
│   └── Results/
│       ├── Result.cs
│       ├── Error.cs
│       └── ResultT.cs
│
├── BuildingBlocks.Infrastructure/                  # EF Core, Outbox e serviços base
│   ├── Persistence/
│   │   ├── SharedDbContext.cs
│   │   ├── Configurations/
│   │   │   └── BaseEntityConfiguration.cs
│   │   └── Interceptors/
│   │       ├── AuditableEntityInterceptor.cs
│   │       ├── SoftDeleteInterceptor.cs
│   │       ├── OptimisticConcurrencyInterceptor.cs
│   │       └── PublishDomainEventsInterceptor.cs
│   ├── Messaging/
│   │   ├── Outbox/
│   │   │   ├── OutboxMessage.cs
│   │   │   └── OutboxInterceptor.cs
│   │   └── Integration/
│   │       └── IEventBus.cs
│   └── Services/
│       └── DateTimeProvider.cs
│
├── modules/                                        # Bounded Contexts
│   ├── users/
│   │   ├── Users.Core/
│   │   ├── Users.Application/
│   │   ├── Users.Infrastructure/
│   │   └── Users.Contracts/
│   ├── catalog/
│   │   ├── Catalog.Core/
│   │   ├── Catalog.Application/
│   │   ├── Catalog.Infrastructure/
│   │   └── Catalog.Contracts/
│   ├── cart/
│   │   ├── Cart.Core/
│   │   ├── Cart.Application/
│   │   ├── Cart.Infrastructure/
│   │   └── Cart.Contracts/
│   ├── orders/
│   │   ├── Orders.Core/
│   │   ├── Orders.Application/
│   │   ├── Orders.Infrastructure/
│   │   └── Orders.Contracts/
│   ├── payments/
│   │   ├── Payments.Core/
│   │   ├── Payments.Application/
│   │   ├── Payments.Infrastructure/
│   │   └── Payments.Contracts/
│   └── coupons/
│       ├── Coupons.Core/
│       ├── Coupons.Application/
│       ├── Coupons.Infrastructure/
│       └── Coupons.Contracts/
│
├── tests/
│   ├── Unit/
│   ├── Integration/
│   ├── E2E/
│   └── Architecture/
│
├── docs/
│   ├── architecture/
│   ├── modules/
│   ├── diagrams/
│   ├── api/
│   ├── db/
│   ├── PRD.md
│   ├── comunicacao-entre-modulos.md
│   └── command-list.md
│
├── docker/
├── script/
├── .gitignore
├── .editorconfig
├── Bcommerce-Monolito.sln
└── README.md
```

---

## Princípios da Estrutura

### 1. Separação em Camadas (Clean Architecture)

```text
Core (Domain) → Application → Infrastructure → Contracts
      ↑                                            ↓
      └────────── Comunicação via Mediator ────────┘
```

**Regras de Dependência**

- Core: não depende de nada
- Application: depende apenas do Core
- Infrastructure: depende de Core e Application
- Contracts: apenas DTOs e abstrações

---

### 2. Building Blocks

- Domain: Entity, AggregateRoot, ValueObject, Domain Events
- Application: Commands, Queries, Behaviors
- Infrastructure: Outbox, Interceptors, Event Bus

---

### 3. Módulos Independentes

- Cada módulo é um **bounded context**
- Comunicação apenas via **Contracts**
- Sem referências diretas entre módulos

---

## Organização de Commands e Queries

```text
Commands/
└── RegisterUser/
    ├── RegisterUserCommand.cs
    ├── RegisterUserCommandHandler.cs
    └── RegisterUserCommandValidator.cs
```

```text
Queries/
└── GetUserById/
    ├── GetUserByIdQuery.cs
    └── GetUserByIdQueryHandler.cs
```

---

## Convenções de Nomenclatura

### Projetos

- Ecommerce.Modules.<Module>.<Layer>
- Ecommerce.BuildingBlocks.<Layer>
- Ecommerce.Shared.<Type>

### Namespaces

```csharp
namespace Ecommerce.Modules.Users.Core.Domain.Entities;
namespace Ecommerce.Modules.Catalog.Application.Commands.CreateProduct;
```

---

## Próximos Passos

1. Criar os projetos (.csproj)
2. Configurar dependências no .sln
3. Implementar Building Blocks
4. Criar Shared.Abstractions
5. Implementar infraestrutura compartilhada
6. Desenvolver módulos

---

**Última atualização**: 2025-12-13
**Versão**: 1.0
