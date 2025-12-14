# Architecture Documentation

Documentação de arquitetura do projeto BCommerce - Modular Monolith.

## 📁 Estrutura

```
architecture/
├── adr/                    # Architecture Decision Records
│   ├── 001-monolito-modular.md
│   ├── 002-cqrs-mediatr.md
│   ├── 003-event-driven.md
│   └── 004-postgresql-schema.md
├── diagrams/               # Diagramas de arquitetura
│   ├── system-context.md
│   ├── module-dependencies.md
│   ├── database-erd.md
│   └── event-flow.md
└── README.md               # Este arquivo
```

## 🏛️ Visão Geral

BCommerce é um **Modular Monolith** construído com:

- **.NET 8** - Framework principal
- **PostgreSQL** - Banco de dados com schemas por módulo
- **MediatR** - CQRS e Mediator pattern
- **EF Core** - ORM
- **ASP.NET Identity** - Autenticação

## 📦 Módulos

| Módulo | Schema | Responsabilidade |
|--------|--------|------------------|
| **Users** | `users` | Autenticação, perfis, sessões |
| **Catalog** | `catalog` | Produtos, categorias, estoque |
| **Cart** | `cart` | Carrinho de compras |
| **Orders** | `orders` | Pedidos e processamento |
| **Payments** | `payments` | Pagamentos e transações |
| **Coupons** | `coupons` | Cupons e promoções |

## 📐 Padrões Utilizados

- **DDD** - Domain-Driven Design
- **CQRS** - Command Query Responsibility Segregation
- **Repository Pattern** - Abstração de acesso a dados
- **Unit of Work** - Gerenciamento de transações
- **Outbox Pattern** - Eventos confiáveis

## 📚 ADRs (Architecture Decision Records)

Decisões de arquitetura documentadas:

1. [Monolito Modular](adr/001-monolito-modular.md) - Por que não microserviços
2. [CQRS com MediatR](adr/002-cqrs-mediatr.md) - Estrutura de Commands/Queries
3. [Event-Driven](adr/003-event-driven.md) - Comunicação entre módulos
4. [PostgreSQL Schema](adr/004-postgresql-schema.md) - Organização do banco

## 📈 Diagramas

- [System Context](diagrams/system-context.md) - Visão de alto nível
- [Module Dependencies](diagrams/module-dependencies.md) - Dependências entre módulos
- [Database ERD](diagrams/database-erd.md) - Modelo de dados
- [Event Flow](diagrams/event-flow.md) - Fluxo de eventos
