# Module Dependencies Diagram

Diagrama de dependências entre os módulos do BCommerce.

## Regras de Dependência

1. **Módulos NÃO dependem diretamente uns dos outros**
2. **Todos os módulos dependem apenas de BuildingBlocks**
3. **Comunicação entre módulos é feita via Integration Events**

## Diagrama de Dependências

```mermaid
graph TB
    subgraph "API Layer"
        API[Bcommerce.Api]
    end
    
    subgraph "Modules"
        Users[Users Module]
        Catalog[Catalog Module]
        Cart[Cart Module]
        Orders[Orders Module]
        Payments[Payments Module]
        Coupons[Coupons Module]
    end
    
    subgraph "Building Blocks"
        Domain[BuildingBlocks.Domain]
        App[BuildingBlocks.Application]
        Infra[BuildingBlocks.Infrastructure]
        Pres[BuildingBlocks.Presentation]
    end
    
    API --> Users
    API --> Catalog
    API --> Cart
    API --> Orders
    API --> Payments
    API --> Coupons
    API --> Pres
    
    Users --> Domain
    Users --> App
    Users --> Infra
    
    Catalog --> Domain
    Catalog --> App
    Catalog --> Infra
    
    Cart --> Domain
    Cart --> App
    Cart --> Infra
    
    Orders --> Domain
    Orders --> App
    Orders --> Infra
    
    Payments --> Domain
    Payments --> App
    Payments --> Infra
    
    Coupons --> Domain
    Coupons --> App
    Coupons --> Infra
    
    Pres --> App
    App --> Domain
    Infra --> Domain
```

## Comunicação via Events

```mermaid
sequenceDiagram
    participant Orders
    participant Outbox
    participant Processor
    participant Payments
    participant Catalog
    
    Orders->>Outbox: OrderPlaced event
    Note over Outbox: Stored in domain_events table
    
    Processor->>Outbox: Poll pending events
    Processor->>Payments: PaymentRequested
    Processor->>Catalog: ReserveStock
    
    Payments->>Outbox: PaymentConfirmed event
    Processor->>Orders: UpdateOrderStatus
```

## Matrix de Dependências

| Módulo | Domain | App | Infra | Presentation |
|--------|--------|-----|-------|--------------|
| Users | ✅ | ✅ | ✅ | ❌ |
| Catalog | ✅ | ✅ | ✅ | ❌ |
| Cart | ✅ | ✅ | ✅ | ❌ |
| Orders | ✅ | ✅ | ✅ | ❌ |
| Payments | ✅ | ✅ | ✅ | ❌ |
| Coupons | ✅ | ✅ | ✅ | ❌ |
| API | ❌ | ✅ | ✅ | ✅ |

## Integration Events

| Evento | Produtor | Consumidores |
|--------|----------|--------------|
| `UserRegistered` | Users | Catalog, Coupons |
| `OrderPlaced` | Orders | Payments, Catalog, Coupons |
| `PaymentConfirmed` | Payments | Orders |
| `PaymentFailed` | Payments | Orders |
| `StockReserved` | Catalog | Orders |
| `StockReleased` | Catalog | Orders, Cart |
| `CouponApplied` | Coupons | Orders |
