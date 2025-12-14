# ADR 003: Arquitetura Event-Driven com Outbox Pattern

## Status

**Aceito** - Dezembro 2024

## Contexto

Módulos precisam se comunicar de forma:
- Desacoplada (sem dependências diretas)
- Confiável (eventos não podem ser perdidos)
- Assíncrona (não bloquear operações principais)
- Rastreável (auditoria de eventos)

## Decisão

Adotamos **Event-Driven Architecture** com **Outbox Pattern** para comunicação entre módulos.

### Tipos de Eventos

```
Events/
├── Domain Events      # Internos ao aggregate (Product.Created)
└── Integration Events # Entre módulos (OrderPlaced, PaymentConfirmed)
```

### Outbox Pattern

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Handler   │────>│   Outbox    │────>│  Processor  │
│  (Command)  │     │   Table     │     │    Job      │
└─────────────┘     └─────────────┘     └─────────────┘
       │                   │                   │
       │ 1. Save entity    │ 2. Save event     │ 3. Publish
       │    + event in     │    in same        │    to handlers
       │    same TX        │    transaction    │
       ▼                   ▼                   ▼
   ┌───────┐         ┌──────────┐       ┌───────────┐
   │ Entity│         │ domain_  │       │ Event     │
   │ Table │         │ events   │       │ Handlers  │
   └───────┘         └──────────┘       └───────────┘
```

### Tabela shared.domain_events

```sql
CREATE TABLE shared.domain_events (
    id UUID PRIMARY KEY,
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(200) NOT NULL,
    event_type VARCHAR(200) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL,
    processed_at TIMESTAMPTZ,
    retry_count INT DEFAULT 0,
    error_message TEXT
);
```

### Fluxo

1. **PublishDomainEventsInterceptor**: Serializa eventos para `domain_events` durante SaveChanges
2. **ProcessOutboxMessagesJob**: BackgroundService que processa eventos pendentes
3. **Integration Event Handlers**: Consomem eventos e executam ações

## Consequências

### Positivas
- ✅ Consistência eventual garantida
- ✅ Desacoplamento entre módulos
- ✅ Rastreabilidade de eventos
- ✅ Retry automático em caso de falhas

### Negativas
- ❌ Complexidade adicional
- ❌ Eventual consistency (não immediate)
- ❌ Background job precisa estar sempre rodando

## Referências

- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Domain Events vs Integration Events](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
