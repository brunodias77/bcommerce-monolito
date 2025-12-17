# Projeto de Referência - Módulo Teste

Este módulo serve como **Implementação de Referência** para a arquitetura monolítica modular do `Bcommerce`. Ele demonstra a integração de todos os `BuildingBlocks` e padrões arquiteturais adotados.

## 🏗 Estrutura do Módulo

O módulo segue a **Clean Architecture** e **DDD**, dividido em 4 camadas principais:

### 1. Domain (`Bcommerce.Modules.ProjetoTeste.Domain`)
Contém o núcleo da lógica de negócios, independente de frameworks externos.
- **Entities**: `TestItem` (Aggregate Root).
- **Events**: `TestItemCreatedEvent` (Domain Event).
- **Repositories Interfaces**: Definidos na camada de Application agora (por decisão de design para evitar dependência de Application no Domain se usar IRepository genérico da Application). *Nota: Na refatoração recente, a interface foi movida para Application.*

### 2. Application (`Bcommerce.Modules.ProjetoTeste.Application`)
Orquestra os casos de uso utilizando padrão **CQRS** (MediatR).
- **Commands**: `CreateTestItemCommand` (Escrita).
- **Queries**: `GetTestItemQuery` (Leitura com Cache).
- **Validators**: FluentValidation para garantir integridade dos inputs.
- **Behaviors**: Pipeline de validação, logging e transação (via BuildingBlocks).

### 3. Infrastructure (`Bcommerce.Modules.ProjetoTeste.Infrastructure`)
Implementa as abstrações e acessa recursos externos.
- **Data**: `TestDbContext` (EF Core), Mapeamentos (`TestItemConfiguration`).
- **Repositories**: `TestItemRepository` (Implementação).
- **Background Jobs**: `SampleJob` (Quartz.NET) para tarefas assíncronas.

### 4. Api (`Bcommerce.Modules.ProjetoTeste.Api`)
A camada de entrada (Entry Point) do módulo, expondo endpoints HTTP.
- **Controllers**: `TestItemsController` (REST).
- **Program.cs**: Configuração de DI e Pipeline.

## 🧱 Integração com BuildingBlocks

Este projeto demonstra o uso dos blocos de construção centrais:

- **Web**: `ApiControllerBase` para padronização de respostas, Global Exception Handling.
- **Data**: `BaseDbContext` com Interceptors (Auditoria, SoftDelete, DomainEvents), `Repository<T>`.
- **CQRS**: `ICommand`, `IQuery`, `ICommandHandler`.
- **Validation**: Integração automática do FluentValidation no pipeline do MediatR.
- **Messaging**: (Preparado para MassTransit/Outbox via DomainEvents).
- **Caching**: Uso de `ICacheService` (Redis/Memory) nas Queries.
- **Observability**: Logging estruturado e métricas (via BuildingBlocks).
- **Security**: Autenticação e Autorização (preparado para JWT).

## 🚀 Como Executar

1. **Configuração**: Garanta que o `appsettings.json` (ou User Secrets) tenha a ConnectionString válida.
2. **Build**: Execute `dotnet build` na raiz ou no projeto Api.
3. **Run**: Execute `dotnet run --project src/Modules/Bcommerce.Modules.ProjetoTeste/Api`.
4. **Swagger**: Acesse `/swagger` para testar os endpoints.

## 📝 Padrões Demonstrados

- **Rich Domain Model**: Entidades com comportamentos e construtores protegidos.
- **Event Sourcing (Parcial)**: Disparo de eventos de domínio para efeitos colaterais.
- **Repository Pattern**: Abstração de acesso a dados.
- **Unit of Work**: Gerenciado automaticamente pelo Pipeline de Transação.
- **Dependency Injection**: Cada camada possui seu `DependencyInjection.cs` para registrar serviços.

---
**Responsável**: Equipe de Arquitetura
**Status**: Referência/Estável
