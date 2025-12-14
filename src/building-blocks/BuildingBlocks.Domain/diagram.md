# Diagramas de Arquitetura - BuildingBlocks.Domain

## 1. Visão Geral da Estrutura

```mermaid
graph TB
    subgraph "BuildingBlocks.Domain"
        Entities[Entities]
        Events[Events]
        Models[Models]
        Repositories[Repositories]
        Exceptions[Exceptions]
    end

    subgraph "Entities"
        Entity[Entity.cs<br/>Id + DomainEvents]
        AggregateRoot[AggregateRoot.cs<br/>Version]
        IAuditable[IAuditableEntity.cs]
        ISoftDelete[ISoftDeletable.cs]
    end

    subgraph "Events"
        IDomainEvent[IDomainEvent.cs]
        DomainEvent[DomainEvent.cs]
        IIntegrationEvent[IIntegrationEvent.cs]
        IntegrationEvent[IntegrationEvent.cs]
    end

    subgraph "Models"
        ValueObject[ValueObject.cs]
        Enumeration[Enumeration.cs]
        Result[Result.cs]
    end

    subgraph "Repositories"
        IRepository[IRepository.cs]
        IUnitOfWork[IUnitOfWork.cs]
    end

    subgraph "Exceptions"
        DomainException[DomainException.cs]
    end

    Entity --> AggregateRoot
    Entity -.implements.-> IAuditable
    Entity -.implements.-> ISoftDelete
    DomainEvent -.implements.-> IDomainEvent
    IntegrationEvent -.implements.-> IIntegrationEvent
```

## 2. Fluxo de Domain Events

```mermaid
sequenceDiagram
    participant Entity as Aggregate Root
    participant DomainEvent as Domain Event
    participant Handler as Event Handler
    participant DB as Database
    participant Outbox as Outbox Table

    Entity->>DomainEvent: AddDomainEvent()
    Entity->>DB: SaveChangesAsync()
    DB->>Handler: Dispatch Events (MediatR)
    Handler->>Handler: Process Logic
    alt Is Integration Event
        Handler->>Outbox: Save to shared.domain_events
        Outbox-->>Handler: Saved
    end
    Handler-->>Entity: Event Processed
```

## 3. Fluxo de Integration Events (Entre Módulos)

```mermaid
graph LR
    subgraph "Módulo Payments"
        Payment[Payment Aggregate]
        PaymentEvent[PaymentCapturedEvent]
        PaymentHandler[EventHandler]
        EventBus1[Event Bus]
    end

    subgraph "Shared Infrastructure"
        Outbox[(shared.domain_events)]
        BackgroundJob[Background Worker]
    end

    subgraph "Módulo Orders"
        IntegrationHandler[Integration Handler]
        Order[Order Aggregate]
    end

    Payment -->|1. Raises| PaymentEvent
    PaymentEvent -->|2. Handled by| PaymentHandler
    PaymentHandler -->|3. Publishes| EventBus1
    EventBus1 -->|4. Saves to| Outbox
    BackgroundJob -->|5. Polls| Outbox
    BackgroundJob -->|6. Dispatches| IntegrationHandler
    IntegrationHandler -->|7. Updates| Order
```

## 4. Hierarchy de Entities

```mermaid
classDiagram
    class Entity {
        +Guid Id
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        #AddDomainEvent(event)
        #RemoveDomainEvent(event)
        +ClearDomainEvents()
        +Equals(other)
        +GetHashCode()
    }

    class AggregateRoot {
        +int Version
        #IncrementVersion()
    }

    class IAuditableEntity {
        <<interface>>
        +DateTime CreatedAt
        +DateTime UpdatedAt
    }

    class ISoftDeletable {
        <<interface>>
        +DateTime? DeletedAt
        +bool IsDeleted
        +Delete()
        +Restore()
    }

    class Product {
        +string Sku
        +string Name
        +decimal Price
        +int Stock
        +ReserveStock(quantity)
        +UpdatePrice(newPrice)
    }

    class Order {
        +string OrderNumber
        +OrderStatus Status
        +decimal Total
        +MarkAsPaid(date)
        +Cancel(reason)
    }

    Entity <|-- AggregateRoot
    Entity ..|> IAuditableEntity
    Entity ..|> ISoftDeletable
    AggregateRoot <|-- Product
    AggregateRoot <|-- Order
```

## 5. Result Pattern Flow

```mermaid
graph TD
    Start[Command Handler] --> Validate{Validação}
    Validate -->|Invalid| FailResult[Result.Fail]
    Validate -->|Valid| Process[Processar Lógica]
    Process --> DomainLogic{Regra de Negócio}
    DomainLogic -->|Violated| FailResult
    DomainLogic -->|Success| SuccessResult[Result.Ok]

    FailResult --> Return1[Return Result.IsFailure = true]
    SuccessResult --> Return2[Return Result.IsSuccess = true]

    Return1 --> Controller[Controller]
    Return2 --> Controller

    Controller --> Check{Check Result}
    Check -->|IsFailure| BadRequest[400 Bad Request]
    Check -->|IsSuccess| OK[200 OK]
```

## 6. Repository Pattern

```mermaid
classDiagram
    class IRepository~T~ {
        <<interface>>
        +IUnitOfWork UnitOfWork
    }

    class IProductRepository {
        <<interface>>
        +GetByIdAsync(id)
        +GetBySkuAsync(sku)
        +AddAsync(product)
        +Update(product)
        +Remove(product)
    }

    class ProductRepository {
        -CatalogDbContext _context
        +GetByIdAsync(id)
        +GetBySkuAsync(sku)
        +AddAsync(product)
        +Update(product)
        +Remove(product)
    }

    class IUnitOfWork {
        <<interface>>
        +SaveChangesAsync()
        +SaveEntitiesAsync()
    }

    class CatalogDbContext {
        +DbSet~Product~ Products
        +SaveChangesAsync()
        +SaveEntitiesAsync()
        -DispatchDomainEvents()
    }

    IRepository~T~ <|.. IProductRepository
    IProductRepository <|.. ProductRepository
    ProductRepository --> CatalogDbContext
    CatalogDbContext ..|> IUnitOfWork
```

## 7. Value Object Equality

```mermaid
graph TD
    VO1[Address Instance 1<br/>Street: Rua A<br/>City: SP<br/>PostalCode: 01234-567]
    VO2[Address Instance 2<br/>Street: Rua A<br/>City: SP<br/>PostalCode: 01234-567]

    Comparison{GetEqualityComponents}

    VO1 --> Comparison
    VO2 --> Comparison

    Comparison --> Compare[Compare All Components]
    Compare --> Result{All Equal?}
    Result -->|Yes| Equal[VO1 == VO2 = TRUE]
    Result -->|No| NotEqual[VO1 == VO2 = FALSE]
```

## 8. Smart Enum Pattern

```mermaid
classDiagram
    class Enumeration {
        +int Id
        +string Name
        +ToString()
        +GetAll~T~()
        +FromId~T~(id)
        +FromName~T~(name)
        +Equals(other)
        +CompareTo(other)
    }

    class OrderStatus {
        +static Pending
        +static Paid
        +static Shipped
        +static Delivered
        +static Cancelled
        +bool CanBeCancelled()
        +bool CanBeShipped()
        +bool IsCompleted()
    }

    class PaymentStatus {
        +static Pending
        +static Authorized
        +static Captured
        +static Failed
        +static Refunded
        +bool CanBeRefunded()
        +bool IsTerminal()
    }

    Enumeration <|-- OrderStatus
    Enumeration <|-- PaymentStatus
```

## 9. Modular Monolith Communication

```mermaid
graph TB
    subgraph "Orders Module"
        OrdersCore[Core<br/>Entities, Events]
        OrdersApp[Application<br/>Commands, Queries]
        OrdersInfra[Infrastructure<br/>Repositories, DbContext]
        OrdersContracts[Contracts<br/>Public API]
    end

    subgraph "Payments Module"
        PaymentsCore[Core<br/>Entities, Events]
        PaymentsApp[Application<br/>Commands, Queries]
        PaymentsInfra[Infrastructure<br/>Repositories, DbContext]
        PaymentsContracts[Contracts<br/>Public API]
    end

    subgraph "Shared"
        Outbox[(domain_events)]
        Mediator[MediatR]
    end

    OrdersApp -->|References| PaymentsContracts
    PaymentsApp -->|Publishes Event| Outbox
    Outbox -->|Background Job| OrdersApp
    OrdersApp -->|Query via| Mediator
    PaymentsApp -->|Handles via| Mediator

    style OrdersContracts fill:#90EE90
    style PaymentsContracts fill:#90EE90
    style Outbox fill:#FFD700
```

## 10. Exception Handling Flow

```mermaid
graph TD
    Request[HTTP Request] --> Middleware[Middleware Pipeline]
    Middleware --> Controller[Controller]
    Controller --> CommandHandler[Command Handler]

    CommandHandler --> DomainLogic[Domain Logic]

    DomainLogic -->|Throws| DomainException
    DomainLogic -->|Returns| Result

    DomainException --> ExceptionMiddleware[Domain Exception Middleware]
    ExceptionMiddleware --> BadRequest1[400 Bad Request<br/>with Error Code]

    Result --> Controller
    Controller -->|IsFailure| BadRequest2[400 Bad Request<br/>with Result.Error]
    Controller -->|IsSuccess| OK[200 OK]
```

---

## Legenda

- 🟢 **Verde**: Contratos públicos (podem ser referenciados por outros módulos)
- 🟡 **Amarelo**: Infraestrutura compartilhada
- 🔵 **Azul**: Componentes internos de módulo
- ➡️ **Setas sólidas**: Dependências diretas
- ⋯➡️ **Setas pontilhadas**: Implements/Uses

---

**Versão**: 1.0.0  
**Data**: 2025-12-13
