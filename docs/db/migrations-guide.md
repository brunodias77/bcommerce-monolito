# Guia de Migrations

Guia para gerenciar migrations do banco de dados BCommerce.

## Estratégia

O BCommerce usa **Code-First Migrations** com EF Core, mas com algumas considerações:

1. **Schema separado por módulo** - Cada módulo tem seu próprio schema PostgreSQL
2. **Migrations por DbContext** - Cada módulo tem seu próprio DbContext e conjunto de migrations
3. **Schema inicial via SQL** - `schema.sql` define a estrutura inicial

## Estrutura

```
src/
├── modules/
│   ├── users/
│   │   └── Users.Infrastructure/
│   │       └── Persistence/
│   │           └── Migrations/
│   ├── catalog/
│   │   └── Catalog.Infrastructure/
│   │       └── Persistence/
│   │           └── Migrations/
│   └── ...
└── docs/
    └── db/
        └── schema.sql          # Schema inicial completo
```

## Comandos EF Core

### Criar Migration

```bash
# Users Module
dotnet ef migrations add <MigrationName> \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext \
    --output-dir Persistence/Migrations

# Catalog Module
dotnet ef migrations add <MigrationName> \
    --project src/modules/catalog/Catalog.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context CatalogDbContext \
    --output-dir Persistence/Migrations
```

### Aplicar Migrations

```bash
# Aplicar todas as migrations pendentes
dotnet ef database update \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext
```

### Gerar Script SQL

```bash
# Script para produção
dotnet ef migrations script \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext \
    --idempotent \
    --output migrations/users_$(date +%Y%m%d).sql
```

### Reverter Migration

```bash
# Reverter para migration específica
dotnet ef database update <MigrationName> \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext
```

## Configuração do DbContext

Cada DbContext deve especificar seu schema e tabela de histórico:

```csharp
services.AddDbContext<UsersDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Tabela de histórico no schema do módulo
        npgsqlOptions.MigrationsHistoryTable(
            "__EFMigrationsHistory", 
            "users"
        );
    });
});
```

## Boas Práticas

### ✅ Faça

1. **Nomes descritivos** para migrations: `AddUserProfileTable`, `AddIndexToProducts`
2. **Migrations pequenas** e focadas
3. **Scripts idempotentes** para produção
4. **Backup antes** de aplicar em produção
5. **Teste em staging** primeiro

### ❌ Evite

1. **Migrations destrutivas** sem plano de rollback
2. **Alterar migrations** já aplicadas em produção
3. **Migrations com dados** de seed em produção
4. **DROP TABLE** sem backup

## Fluxo para Produção

```
1. Desenvolver e testar localmente
   ↓
2. Gerar script SQL
   ↓
3. Review do script
   ↓
4. Aplicar em staging
   ↓
5. Testar em staging
   ↓
6. Aplicar em produção (com backup)
   ↓
7. Verificar integridade
```

## Setup Inicial

Para criar o banco de dados do zero:

```bash
# 1. Criar banco de dados
createdb bcommerce

# 2. Aplicar schema inicial
psql -d bcommerce -f docs/db/schema.sql

# 3. Ou usar migrations (se disponíveis)
dotnet ef database update --context UsersDbContext
dotnet ef database update --context CatalogDbContext
# ... outros módulos
```

## Troubleshooting

### Erro: "Relation already exists"

O schema foi criado por outro método. Use `--idempotent`:

```bash
dotnet ef migrations script --idempotent
```

### Erro: "Migration not found"

Verifique se o projeto está correto:

```bash
dotnet ef migrations list \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext
```

### Conflito de Schema

Se dois módulos tentarem criar o mesmo objeto:

1. Mova para `shared` schema
2. Crie migration no módulo que "possui" o objeto
3. Outros módulos devem referenciar, não criar

## Referências

- [EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Schemas](https://www.postgresql.org/docs/current/ddl-schemas.html)
