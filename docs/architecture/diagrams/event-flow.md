# Event Flow Diagram

Diagrama de fluxo de eventos no sistema BCommerce.

## Visão Geral

```mermaid
flowchart TB
    subgraph "Domain Layer"
        Entity[Entity/Aggregate]
        DomainEvent[Domain Event]
    end
    
    subgraph "Infrastructure Layer"
        Interceptor[PublishDomainEventsInterceptor]
        Outbox[(domain_events table)]
        Processor[ProcessOutboxMessagesJob]
    end
    
    subgraph "Application Layer"
        Handler[Integration Event Handler]
        Command[Command Handler]
    end
    
    Entity -->|raises| DomainEvent
    DomainEvent -->|serialized by| Interceptor
    Interceptor -->|saves to| Outbox
    Processor -->|reads from| Outbox
    Processor -->|publishes to| Handler
    Handler -->|may trigger| Command
```

## Fluxo Detalhado: Order Checkout

```mermaid
sequenceDiagram
    autonumber
    
    participant Client
    participant API
    participant Orders
    participant Outbox
    participant Processor
    participant Payments
    participant Catalog
    participant Email
    
    Client->>API: POST /api/orders/checkout
    API->>Orders: PlaceOrderCommand
    
    Orders->>Orders: Create Order aggregate
    Orders->>Orders: Raise OrderPlacedEvent
    Orders->>Outbox: Save Order + Event (same TX)
    Orders-->>API: Order ID
    API-->>Client: 201 Created
    
    Note over Processor: Background Job (every 2s)
    
    Processor->>Outbox: Get pending events
    Outbox-->>Processor: OrderPlacedEvent
    
    par Parallel Processing
        Processor->>Payments: ProcessPaymentCommand
        Payments->>Payments: Authorize payment
        Payments->>Outbox: PaymentAuthorizedEvent
        
        Processor->>Catalog: ReserveStockCommand
        Catalog->>Catalog: Reserve items
        Catalog->>Outbox: StockReservedEvent
    end
    
    Processor->>Outbox: Mark as processed
    
    Processor->>Outbox: Get PaymentAuthorizedEvent
    Processor->>Orders: UpdateOrderStatusCommand
    Processor->>Email: SendOrderConfirmationCommand
```

## Tipos de Eventos

### Domain Events (Internos ao Aggregate)

| Evento | Aggregate | Descrição |
|--------|-----------|-----------|
| `OrderCreated` | Order | Pedido criado |
| `OrderItemAdded` | Order | Item adicionado |
| `OrderStatusChanged` | Order | Status alterado |
| `ProductCreated` | Product | Produto criado |
| `StockUpdated` | Product | Estoque atualizado |

### Integration Events (Entre Módulos)

| Evento | Producer | Consumers | Descrição |
|--------|----------|-----------|-----------|
| `OrderPlaced` | Orders | Payments, Catalog, Coupons | Pedido finalizado |
| `PaymentConfirmed` | Payments | Orders | Pagamento aprovado |
| `PaymentFailed` | Payments | Orders | Pagamento falhou |
| `StockReserved` | Catalog | Orders | Estoque reservado |
| `StockReleased` | Catalog | Orders | Estoque liberado |
| `UserRegistered` | Users | Catalog, Coupons | Novo usuário |

## Outbox Pattern Flow

```
1. Handler executa Command
   ↓
2. Aggregate levanta Domain Event via RaiseDomainEvent()
   ↓
3. SaveChangesAsync() é chamado
   ↓
4. PublishDomainEventsInterceptor intercepta
   ↓
5. Serializa eventos para domain_events table
   ↓
6. Commit da transação (Entity + Events atômico)
   ↓
7. ProcessOutboxMessagesJob (background)
   ↓
8. Lê eventos pendentes (processed_at IS NULL)
   ↓
9. Deserializa e publica para handlers registrados
   ↓
10. Marca como processado ou incrementa retry_count
```

## Garantias

| Garantia | Implementação |
|----------|---------------|
| **At-least-once delivery** | Retry com backoff |
| **Ordering** | Por aggregate_id |
| **Idempotency** | Handlers devem ser idempotentes |
| **Durability** | Eventos persistidos em DB |
