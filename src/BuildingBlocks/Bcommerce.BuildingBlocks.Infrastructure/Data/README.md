# Data Infrastructure (EF Core)

Este diretório contém a base para a persistência de dados utilizando Entity Framework Core. Ele fornece abstrações e implementações padrão para Repositórios, Unit of Work e configurações automáticas (Interceptors/Converters).

## Core

### `BaseDbContext.cs`
**Responsabilidade:** Servir como classe base para todos os DbContexts dos módulos da aplicação.
**Por que existe:** Para centralizar a configuração de infraestrutura transversal, como Interceptors (Auditoria, Soft Delete), carregamento de mapeamentos e configurações globais.
**Em que situação usar:** Herde desta classe ao criar um novo DbContext (ex: `CatalogDbContext : BaseDbContext`).
**O que pode dar errado:** Esquecer de chamar `base.OnConfiguring` ou `base.OnModelCreating`, o que faria com que os interceptors ou configurações globais não fossem aplicados.
**Exemplo real de uso:**
```csharp
public class SalesDbContext(DbContextOptions opts, ...) : BaseDbContext(opts, ...) {
    public DbSet<Pedido> Pedidos { get; set; }
}
```

### `ReadRepository.cs`
**Responsabilidade:** Repositório otimizado exclusivamente para leitura (Queries).
**Por que existe:** Para separar a leitura da escrita (CQRS lite). As consultas usam `AsNoTracking()` por padrão, o que é muito mais rápido e gasta menos memória do que carregar entidades rastreadas.
**Em que situação usar:** Em Queries (MediatR) ou serviços que apenas precisam exibir dados.
**O que pode dar errado:** TENTAR alterar uma entidade retornada por este repositório. O EF Core não detectará a mudança e o `SaveChanges` não fará nada.
**Exemplo real de uso:**
```csharp
var produto = await _readRepo.GetByIdAsync(id); // Rápido, sem cache do EF
```

### `Repository.cs`
**Responsabilidade:** Repositório de escrita (Commands) focado em Agregados.
**Por que existe:** Para garantir que apenas Raízes de Agregado sejam persistidas, mantendo a consistência do limite transacional.
**Em que situação usar:** Em Commands (MediatR) que precisam alterar o estado do sistema.
**O que pode dar errado:** Tentar usar este repositório para query complexa de relatórios. Ele carrega todo o grafo da entidade (tracking), o que é pesado.
**Exemplo real de uso:**
```csharp
var pedido = await _repo.GetByIdAsync(id);
pedido.AdicionarItem(...);
await _repo.UnitOfWork.SaveChangesAsync();
```

### `UnitOfWork.cs`
**Responsabilidade:** Encapsular a transação do banco de dados.
**Por que existe:** Para permitir que múltiplas operações de repositório (ex: salvar Pedido e baixar Estoque) sejam comitadas em uma única transação atômica.
**Em que situação usar:** Injetado nos Command Handlers.
**O que pode dar errado:** Esquecer de chamar `SaveChangesAsync()`. As mudanças ficarão apenas em memória e serão perdidas ao fim da requisição.
**Exemplo real de uso:**
```csharp
await _uow.SaveChangesAsync(cancellationToken);
```

---

## Interceptors

### `AuditableEntityInterceptor.cs`
**Responsabilidade:** Preencher automaticamente `CreatedAt` e `UpdatedAt`.
**Por que existe:** Para garantir que nenhuma entidade seja salva sem data de criação/alteração, removendo essa responsabilidade da regra de negócio.
**Em que situação usar:** Ativado automaticamente no `BaseDbContext`.
**O que pode dar errado:** Se o servidor estiver com relógio errado, todas as datas ficarão erradas. Ele usa `IDateTimeProvider`, então certifique-se de que a implementação injetada esteja correta.
**Exemplo real de uso:**
```csharp
// No insert: CreatedAt = Now, UpdatedAt = Now
// No update: UpdatedAt = Now
```

### `SoftDeleteInterceptor.cs`
**Responsabilidade:** Interceptar comandos `DELETE` e transformá-los em `UPDATE IsDeleted = 1`.
**Por que existe:** Para implementar Exclusão Lógica de forma transparente. O desenvolvedor chama `Remove()`, mas o dado não é apagado fisicamente.
**Em que situação usar:** Ativado para entidades que implementam `ISoftDeletable`.
**O que pode dar errado:** Se for necessário realmente apagar fisicamente (ex: LGPD), este interceptor impedirá. Nesses casos, é necessário desativá-lo temporariamente ou usar SQL direto.
**Exemplo real de uso:**
```csharp
_context.Pedidos.Remove(p); // Vira UPDATE Pedidos SET IsDeleted=1...
```

### `OptimisticLockInterceptor.cs`
**Responsabilidade:** Incrementar a versão da entidade em cada atualização.
**Por que existe:** Para implementar Concorrência Otimista (Optimistic Concurrency Control) e evitar Lost Updates.
**Em que situação usar:** Ativado para entidades `IVersionable`.
**O que pode dar errado:** A interface de usuário precisa estar preparada para tratar o erro de concorrência e pedir para o usuário recarregar a tela.
**Exemplo real de uso:**
```csharp
// Update Pedido SET Status = 'X', Version = Version + 1 WHERE Id = '...' AND Version = 1
```

### `DomainEventInterceptor.cs`
**Responsabilidade:** Disparar eventos de domínio armazenados na entidade.
**Por que existe:** Para garantir que os side-effects (ex: enviar email) só ocorram se a transação de banco for bem sucedida (Eventual Consistency in-process).
**Em que situação usar:** Alternativa simples ao Outbox Pattern.
**O que pode dar errado:** Se o handler do evento falhar, pode, dependendo da configuração, reverter a transação original ou deixar o sistema em estado inconsistente se o evento for pós-commit.
**Exemplo real de uso:**
```csharp
// Após SaveChanges, percorre entidades e publica eventos via MediatR
```

---

## Configurations & Converters

### `EntityConfiguration.cs` / `AggregateRootConfiguration.cs`
**Responsabilidade:** Aplicar configurações padrão do EF Core (mapeamento).
**Por que existe:** Para evitar repetição de código (`builder.Property(x => x.CreatedAt).IsRequired()`) em cada classe de mapeamento.
**Exemplo real de uso:**
```csharp
public class ProdutoCfg : AggregateRootConfiguration<Produto> { ... }
```

### `DateTimeUtcConverter.cs`
**Responsabilidade:** Forçar que todo `DateTime` lido/gravado seja UTC.
**Por que existe:** O PostgreSQL e outros bancos podem salvar sem Timezone. O C# `DateTime` tem `Kind`. Isso garante que sempre trataremos como UTC.
**Exemplo real de uso:**
```csharp
builder.Property(x => x.Data).HasConversion<DateTimeUtcConverter>();
```

### `EnumerationConverter.cs`
**Responsabilidade:** Mapear classes `Enumeration` (Smart Enum) para inteiros no banco.
**Por que existe:** O EF Core sabe lidar com `enum` padrão, mas não com classes que simulam enums.
**Exemplo real de uso:**
```csharp
builder.Property(x => x.Status).HasConversion<EnumerationConverter<StatusPedido>>();
```

### `ValueObjectConverter.cs`
**Responsabilidade:** Serializar objetos complexos como JSON em uma coluna string.
**Por que existe:** Fallback para armazenar Value Objects que não justificam uma tabela separada (Owned Type) ou estrutura complexa.
**Exemplo real de uso:**
```csharp
builder.Property(x => x.Metadados).HasConversion<ValueObjectConverter<Dic>>();
```
