# ADR 004: PostgreSQL com Schemas por Módulo

## Status

**Aceito** - Dezembro 2024

## Contexto

Precisamos de uma estratégia de banco de dados que:
- Suporte separação lógica entre módulos
- Permita FKs reais para integridade referencial
- Facilite backup e manutenção
- Seja familiar para a equipe

## Decisão

Adotamos **PostgreSQL** com **schemas separados por módulo** em um único banco de dados.

### Estrutura de Schemas

```sql
CREATE SCHEMA IF NOT EXISTS users;    -- Usuários, autenticação
CREATE SCHEMA IF NOT EXISTS catalog;  -- Produtos, categorias
CREATE SCHEMA IF NOT EXISTS cart;     -- Carrinho
CREATE SCHEMA IF NOT EXISTS orders;   -- Pedidos
CREATE SCHEMA IF NOT EXISTS payments; -- Pagamentos
CREATE SCHEMA IF NOT EXISTS coupons;  -- Cupons
CREATE SCHEMA IF NOT EXISTS shared;   -- Outbox, audit log
```

### Convenções

| Aspecto | Convenção |
|---------|-----------|
| Nomes de tabelas | snake_case plural (`users.addresses`) |
| Colunas | snake_case (`created_at`) |
| PKs | UUID v4 |
| FKs | `{table}_id` |
| Índices | `idx_{table}_{columns}` |
| Constraints | `chk_{table}_{constraint}` |

### Tipos Compartilhados

```sql
-- ENUMs no schema shared
CREATE TYPE shared.order_status AS ENUM (...);
CREATE TYPE shared.payment_status AS ENUM (...);
```

### Foreign Keys Cross-Schema

```sql
-- Permitido: FK de orders para users
ALTER TABLE orders.orders
    ADD CONSTRAINT fk_orders_user
    FOREIGN KEY (user_id) REFERENCES users.asp_net_users(id);
```

## Consequências

### Positivas
- ✅ Integridade referencial real
- ✅ Separação lógica clara
- ✅ Backup/restore por schema
- ✅ Queries eficientes com JOINs

### Negativas
- ❌ Mais difícil extrair módulo para microserviço
- ❌ Single database = single point of failure
- ❌ Escala vertical limitada

## Migrations

Cada módulo gerencia suas próprias migrations:

```
migrations/
├── users/
│   ├── 001_create_users_schema.sql
│   └── 002_add_profiles_table.sql
├── catalog/
│   └── ...
└── shared/
    └── 001_create_outbox_table.sql
```

## Referências

- [PostgreSQL Schemas](https://www.postgresql.org/docs/current/ddl-schemas.html)
- [EF Core Multiple Schemas](https://docs.microsoft.com/en-us/ef/core/modeling/entity-types?tabs=data-annotations#table-schema)
