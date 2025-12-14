# CLAUDE.md - AI Assistant Guide

**BCommerce Modular Monolith E-commerce Platform**

Version: 1.0
Last Updated: 2025-12-14
Tech Stack: .NET 8, PostgreSQL, EF Core, MediatR, CQRS, DDD

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Codebase Structure](#codebase-structure)
4. [Development Setup](#development-setup)
5. [Module Structure](#module-structure)
6. [Building Blocks](#building-blocks)
7. [Database Management](#database-management)
8. [Development Workflows](#development-workflows)
9. [Key Patterns & Conventions](#key-patterns--conventions)
10. [Testing Guidelines](#testing-guidelines)
11. [AI Assistant Guidelines](#ai-assistant-guidelines)

---

## Project Overview

BCommerce is a **Modular Monolith** e-commerce platform built with .NET 8 following Domain-Driven Design (DDD) and CQRS principles.

### Key Features
- **Modular Architecture**: 6 independent business modules with clear boundaries
- **DDD**: Rich domain models, aggregates, value objects, and domain events
- **CQRS**: Separate commands and queries using MediatR
- **Event-Driven**: Module communication via integration events (Outbox Pattern)
- **PostgreSQL**: Database with schema-per-module isolation
- **Clean Architecture**: Domain → Application → Infrastructure → Presentation

### Business Modules
1. **Users** - Authentication, profiles, sessions, notifications
2. **Catalog** - Products, categories, inventory, reviews
3. **Cart** - Shopping cart management
4. **Orders** - Order processing and fulfillment
5. **Payments** - Payment processing (PIX, Credit Card, Boleto)
6. **Coupons** - Discount coupons and promotions

### Implementation Status

**Current State (as of 2025-12-13):**

| Component | Status | Completion |
|-----------|--------|------------|
| BuildingBlocks.Domain | ✅ Complete | 100% |
| BuildingBlocks.Application | 🚧 Partial | 14% |
| BuildingBlocks.Infrastructure | 🚧 Configured | 0% |
| Users.Core | ✅ Complete | 100% |
| Users.Infrastructure | 🚧 Partial | 50% |
| Other Modules | ⏳ Planned | 0% |

See `IMPLEMENTATION_STATUS.md` for detailed progress.

---

## Architecture

### Architectural Style

**Modular Monolith** - A single deployable application with logical module separation.

#### Why Modular Monolith? (ADR 001)
- Faster initial development vs microservices
- Simpler deployment and debugging
- ACID transactions across modules
- Clear path to microservices migration
- Suitable for small teams

### Architectural Principles

1. **Module Isolation**: Each module is logically independent
2. **Schema Separation**: Each module has its own PostgreSQL schema
3. **Event-Driven Communication**: Modules communicate via integration events
4. **Unidirectional Dependencies**: Modules depend only on building-blocks
5. **Single API Gateway**: One API project exposes all endpoints

### Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | .NET 8 / ASP.NET Core |
| Database | PostgreSQL 15+ |
| ORM | Entity Framework Core 8+ |
| Mediator | MediatR 12.2.0 |
| Validation | FluentValidation 11.9.0 |
| Mapping | AutoMapper 12.0.1 |
| Authentication | ASP.NET Core Identity |
| Caching | Redis (planned) |
| Search | ElasticSearch (planned) |

### PostgreSQL Extensions
- `uuid-ossp` - UUID generation
- `citext` - Case-insensitive text
- `pg_trgm` - Text similarity search

---

## Codebase Structure

```
bcommerce-monolito/
│
├── docs/                           # Documentation
│   ├── architecture/               # Architecture docs & ADRs
│   │   ├── adr/                   # Architecture Decision Records
│   │   │   ├── 001-monolito-modular.md
│   │   │   ├── 002-cqrs-mediatr.md
│   │   │   ├── 003-event-driven.md
│   │   │   └── 004-postgresql-schema.md
│   │   ├── diagrams/              # System diagrams
│   │   └── README.md
│   ├── api/                       # API documentation
│   │   ├── endpoints.md
│   │   ├── openapi.yaml
│   │   └── postman-collection.json
│   ├── db/                        # Database documentation
│   │   ├── schema.sql             # Complete DB schema
│   │   └── migrations-guide.md
│   └── PRD.md                     # Product Requirements Document
│
├── docker/
│   └── docker-compose.yml         # PostgreSQL container
│
├── script/                        # Utility scripts
│   ├── up.sh                     # Start Docker containers
│   ├── stop.sh                   # Stop containers
│   ├── clean.sh                  # Clean containers
│   ├── create-tables.sh          # Initialize database
│   └── status.sh                 # Check status
│
├── src/
│   ├── api/
│   │   └── Bcommerce.Api/        # Single API project
│   │       ├── Controllers/
│   │       ├── Configurations/
│   │       │   ├── ApplicationDependencyInjection.cs
│   │       │   └── InfraDependencyInjection.cs
│   │       ├── Program.cs
│   │       └── appsettings.json
│   │
│   ├── building-blocks/           # Shared libraries
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   └── BuildingBlocks.Presentation/
│   │
│   └── modules/                   # Business modules
│       ├── users/
│       │   ├── Users.Core/       # Domain layer
│       │   ├── Users.Application/ # Use cases
│       │   ├── Users.Infrastructure/ # Persistence
│       │   ├── Users.Contracts/  # Integration events
│       │   └── Users.Presentation/ # Controllers
│       ├── catalog/              # (Planned)
│       ├── cart/                 # (Planned)
│       ├── orders/               # (Planned)
│       ├── payments/             # (Planned)
│       └── coupons/              # (Planned)
│
├── Bcommerce-Monolito.sln        # Solution file
├── IMPLEMENTATION_STATUS.md       # Implementation progress
├── .gitignore
└── CLAUDE.md                     # This file
```

---

## Development Setup

### Prerequisites

- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL client tools (optional)
- VS Code or Visual Studio 2022

### Quick Start

```bash
# 1. Clone the repository
git clone <repository-url>
cd bcommerce-monolito

# 2. Start PostgreSQL
./script/up.sh
# or
docker compose -f docker/docker-compose.yml up -d

# 3. Verify database is running
./script/status.sh

# 4. Initialize database (if needed)
./script/create-tables.sh

# 5. Restore dependencies
dotnet restore

# 6. Build the solution
dotnet build

# 7. Run the API
cd src/api/Bcommerce.Api
dotnet run

# API will be available at:
# - HTTPS: https://localhost:7000
# - HTTP: http://localhost:5000
# - Swagger: http://localhost:5000
```

### Database Connection

**Connection String:**
```
Host=localhost;Port=5438;Database=bcommerce_db;Username=bcommerce;Password=bcommerce
```

**Access via psql:**
```bash
psql -h localhost -p 5438 -U bcommerce -d bcommerce_db
```

### Project Commands

```bash
# Build specific project
dotnet build src/building-blocks/BuildingBlocks.Domain/

# Run tests (when available)
dotnet test

# Clean build artifacts
dotnet clean

# Add migration (example for Users module)
dotnet ef migrations add MigrationName \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext \
    --output-dir Persistence/Migrations

# Update database
dotnet ef database update \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext
```

---

## Module Structure

Each module follows a **vertical slice architecture** with 4-5 layers:

### Standard Module Layout

```
ModuleName/
│
├── ModuleName.Core/              # Domain Layer (no external dependencies)
│   ├── Entities/                # Aggregates and entities
│   ├── ValueObjects/            # Value objects
│   ├── Events/                  # Domain events
│   ├── Enums/                   # Enumerations
│   ├── Repositories/            # Repository interfaces
│   ├── Exceptions/              # Domain exceptions
│   └── ModuleName.Core.csproj
│
├── ModuleName.Application/       # Application Layer (use cases)
│   ├── Commands/                # Write operations (CQRS)
│   │   └── CommandName/
│   │       ├── CommandNameCommand.cs
│   │       ├── CommandNameCommandHandler.cs
│   │       └── CommandNameCommandValidator.cs
│   ├── Queries/                 # Read operations (CQRS)
│   │   └── QueryName/
│   │       ├── QueryNameQuery.cs
│   │       └── QueryNameQueryHandler.cs
│   ├── DTOs/                    # Data Transfer Objects
│   ├── EventHandlers/           # Domain event handlers
│   ├── Services/                # Service interfaces
│   └── ModuleName.Application.csproj
│
├── ModuleName.Infrastructure/    # Infrastructure Layer
│   ├── Persistence/
│   │   ├── Configurations/      # EF Core configurations
│   │   ├── Migrations/          # EF Core migrations
│   │   └── ModuleNameDbContext.cs
│   ├── Repositories/            # Repository implementations
│   ├── Services/                # External service implementations
│   ├── DependencyInjection.cs   # DI registration
│   └── ModuleName.Infrastructure.csproj
│
├── ModuleName.Contracts/         # Integration Events (public API)
│   ├── Events/
│   │   └── SomethingHappenedIntegrationEvent.cs
│   └── ModuleName.Contracts.csproj
│
└── ModuleName.Presentation/      # Presentation Layer (API)
    ├── Controllers/
    │   └── ModuleNameController.cs
    ├── Requests/                # Request models
    ├── DependencyInjection.cs
    └── ModuleName.Presentation.csproj
```

### Layer Dependencies

```
Presentation → Application → Infrastructure → Core
                    ↓
                Contracts (Integration Events)
```

**Rules:**
- Core has NO dependencies (pure domain)
- Application depends on Core
- Infrastructure depends on Core & Application
- Presentation depends on Application
- Contracts is independent (DTOs only)

---

## Building Blocks

Shared libraries used across all modules.

### BuildingBlocks.Domain (100% Complete)

**Purpose:** Core DDD building blocks - entities, value objects, events, repositories.

#### Key Components

**Entities:**
```csharp
// Base entity with Id and domain events
public abstract class Entity
{
    public Guid Id { get; protected set; }
    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
}

// Aggregate root with versioning for optimistic concurrency
public abstract class AggregateRoot : Entity
{
    public int Version { get; protected set; }
}

// Marker interfaces for cross-cutting concerns
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
}

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; }
}
```

**Value Objects:**
```csharp
// Base for value objects with structural equality
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    // Equals, GetHashCode, ==, != implemented
}

// Example: Money value object
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    // Operators: +, -, *, /
}
```

**Events:**
```csharp
// Domain Event (internal to module)
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

// Integration Event (between modules)
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
```

**Specifications:**
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}

// Composable: And, Or, Not
var spec = new ActiveProductSpec()
    .And(new InStockSpec())
    .And(new PriceRangeSpec(min, max));
```

See `src/building-blocks/BuildingBlocks.Domain/README.md` for complete documentation.

### BuildingBlocks.Application (Partial)

**Purpose:** CQRS abstractions, behaviors, result types, pagination.

#### Implemented (5/37 files)

**Result Pattern:**
```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Ok() => new(true, Error.None);
    public static Result Fail(Error error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
}

// Usage in handlers
public async Task<Result<Product>> Handle(CreateProductCommand request)
{
    if (string.IsNullOrEmpty(request.Name))
        return Result.Fail<Product>(Error.Validation("Name is required"));

    var product = Product.Create(request.Name, request.Price);
    await _repository.AddAsync(product);

    return Result.Ok(product);
}
```

#### Pending Implementation

- Commands & Queries interfaces
- MediatR pipeline behaviors (Validation, Logging, Transaction)
- Pagination abstractions
- Application exceptions
- Validators & mapping profiles

### BuildingBlocks.Infrastructure (Configured)

**Purpose:** EF Core base classes, interceptors, outbox pattern, caching.

#### Pending Implementation (Priority Order)

1. **Persistence Base Classes**
   - `ModuleDbContext` - Base DbContext with interceptors
   - `RepositoryBase<T>` - Generic repository implementation
   - `EntityConfigurationBase<T>` - Common EF configurations

2. **Interceptors**
   - `AuditableEntityInterceptor` - Auto-set CreatedAt/UpdatedAt
   - `SoftDeleteInterceptor` - Filter soft-deleted entities
   - `DomainEventInterceptor` - Dispatch domain events
   - `OptimisticConcurrencyInterceptor` - Handle version conflicts

3. **Outbox Pattern** (for Integration Events)
   - `OutboxMessage` - Outbox table entity
   - `OutboxProcessor` - Background job to process events
   - `OutboxEventBus` - Reliable event publishing

4. **Inbox Pattern** (for Idempotency)
   - `InboxMessage` - Inbox table entity
   - `InboxProcessor` - Deduplication processor

### BuildingBlocks.Presentation (Complete)

**Purpose:** API base controllers, filters, middleware.

**Components:**
- `ApiControllerBase` - Base controller with common logic
- `ExceptionHandlingFilter` - Global exception handling
- `ExceptionHandlingMiddleware` - Alternative middleware approach
- `RequestLoggingMiddleware` - HTTP request logging
- `ResultExtensions` - Convert Result to IActionResult

---

## Database Management

### Schema Organization

PostgreSQL database with **schema-per-module** isolation:

```sql
-- Shared schema for cross-module concerns
CREATE SCHEMA shared;
  -- Tables: domain_events, audit_logs, processed_events

-- Module schemas
CREATE SCHEMA users;     -- Users module tables
CREATE SCHEMA catalog;   -- Catalog module tables
CREATE SCHEMA cart;      -- Cart module tables
CREATE SCHEMA orders;    -- Orders module tables
CREATE SCHEMA payments;  -- Payments module tables
CREATE SCHEMA coupons;   -- Coupons module tables
```

### Key Tables (Shared Schema)

**Outbox Pattern:**
```sql
shared.domain_events (
  id UUID PRIMARY KEY,
  event_type VARCHAR(255),
  payload JSONB,
  module VARCHAR(50),
  occurred_on TIMESTAMPTZ,
  processed_at TIMESTAMPTZ
)
```

**Audit Log:**
```sql
shared.audit_logs (
  id UUID PRIMARY KEY,
  entity_type VARCHAR(255),
  entity_id UUID,
  action VARCHAR(50),
  old_values JSONB,
  new_values JSONB,
  user_id UUID,
  occurred_at TIMESTAMPTZ
)
```

### Database Conventions

**Naming:**
- Tables: `snake_case` (e.g., `user_profiles`)
- Columns: `snake_case` (e.g., `created_at`)
- Primary keys: `id UUID PRIMARY KEY DEFAULT gen_random_uuid()`
- Foreign keys: `{entity}_id` (e.g., `user_id`)

**Standard Columns:**
```sql
-- All aggregate roots
id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
version INT DEFAULT 1 NOT NULL,

-- Auditable entities
created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
updated_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,

-- Soft deletable entities
deleted_at TIMESTAMPTZ
```

**Triggers:**
- `trigger_set_timestamp()` - Auto-update `updated_at`
- `trigger_increment_version()` - Auto-increment version
- `trigger_prevent_delete()` - Enforce soft delete

### Migrations Strategy

**Code-First with EF Core:**

Each module has its own `DbContext` and migration history:

```bash
# Create migration for Users module
dotnet ef migrations add AddUserProfiles \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext \
    --output-dir Persistence/Migrations

# Apply migrations
dotnet ef database update \
    --project src/modules/users/Users.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context UsersDbContext
```

**Migration History Per Module:**
```csharp
// In UsersDbContext configuration
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.MigrationsHistoryTable(
        "__EFMigrationsHistory",
        "users"  // Schema-specific history table
    );
});
```

**Production Deployment:**
1. Generate idempotent SQL scripts
2. Review scripts for safety
3. Test in staging environment
4. Backup production database
5. Apply scripts during maintenance window
6. Verify integrity

See `docs/db/migrations-guide.md` for detailed instructions.

---

## Development Workflows

### Creating a New Feature

**Example: Add "UpdateProfile" feature to Users module**

1. **Create Command** (`Users.Application/Commands/UpdateProfile/`)
```csharp
// UpdateProfileCommand.cs
public record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string PhoneNumber
) : ICommand;

// UpdateProfileCommandValidator.cs
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).Matches(@"^\+?[1-9]\d{1,14}$");
    }
}

// UpdateProfileCommandHandler.cs
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId, ct);
        if (profile is null)
            return Result.Fail(Error.NotFound("Profile not found"));

        profile.Update(request.FullName, request.PhoneNumber);
        _repository.Update(profile);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
```

2. **Add Domain Logic** (`Users.Core/Entities/Profile.cs`)
```csharp
public class Profile : AggregateRoot, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public string FullName { get; private set; }
    public string PhoneNumber { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Update(string fullName, string phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProfileUpdatedEvent(Id, UserId));
    }
}
```

3. **Create Controller Endpoint** (`Users.Presentation/Controllers/ProfileController.cs`)
```csharp
[ApiController]
[Route("api/users/profile")]
public class ProfileController : ApiControllerBase
{
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var command = new UpdateProfileCommand(
            User.GetUserId(), // From JWT
            request.FullName,
            request.PhoneNumber
        );

        var result = await Sender.Send(command, ct);

        return result.Match(
            onSuccess: () => Ok(),
            onFailure: error => BadRequest(error)
        );
    }
}
```

4. **Test the Feature**
```bash
# Manual test via HTTP file
POST https://localhost:7000/api/users/profile
Content-Type: application/json

{
  "fullName": "John Doe",
  "phoneNumber": "+5511999999999"
}
```

### Creating a New Module

**Example: Create "Notifications" module**

1. **Create Project Structure**
```bash
mkdir -p src/modules/notifications/{Notifications.Core,Notifications.Application,Notifications.Infrastructure,Notifications.Contracts,Notifications.Presentation}
```

2. **Create .csproj files** for each layer (copy from Users module)

3. **Add to Solution**
```bash
dotnet sln add src/modules/notifications/Notifications.Core
dotnet sln add src/modules/notifications/Notifications.Application
# ... etc
```

4. **Create DbContext** (`Notifications.Infrastructure/Persistence/NotificationsDbContext.cs`)
```csharp
public class NotificationsDbContext : DbContext, IUnitOfWork
{
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }
}
```

5. **Register in DI** (`Bcommerce.Api/Configurations/InfraDependencyInjection.cs`)
```csharp
services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notifications")
    )
);
```

6. **Create Initial Migration**
```bash
dotnet ef migrations add InitialCreate \
    --project src/modules/notifications/Notifications.Infrastructure \
    --startup-project src/api/Bcommerce.Api \
    --context NotificationsDbContext \
    --output-dir Persistence/Migrations
```

### Handling Integration Events

**Publishing an Event (from Orders module):**
```csharp
// 1. Define event in Orders.Contracts
public record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    DateTime CreatedAt
) : IntegrationEvent("orders");

// 2. Publish in OrderCreatedEventHandler
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IEventBus _eventBus;

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken ct)
    {
        var integrationEvent = new OrderCreatedIntegrationEvent(
            notification.OrderId,
            notification.UserId,
            notification.TotalAmount,
            notification.CreatedAt
        );

        await _eventBus.PublishAsync(integrationEvent, ct);
    }
}
```

**Consuming an Event (in Notifications module):**
```csharp
public class OrderCreatedIntegrationEventHandler
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly INotificationRepository _repository;

    public async Task Handle(OrderCreatedIntegrationEvent @event, CancellationToken ct)
    {
        var notification = Notification.Create(
            @event.UserId,
            "Order Created",
            $"Your order #{@event.OrderId} was created successfully!"
        );

        await _repository.AddAsync(notification, ct);
    }
}
```

---

## Key Patterns & Conventions

### Domain-Driven Design (DDD)

**Aggregates:**
- Enforce invariants within aggregate boundary
- External entities reference aggregates by ID only
- Aggregate root is the entry point for all modifications

**Value Objects:**
- Immutable objects defined by their properties
- No identity, only structural equality
- Examples: Money, Address, DateRange

**Domain Events:**
- Capture important business events
- Published within the same transaction
- Handled by event handlers in the same module

**Integration Events:**
- Cross-module communication
- Stored in Outbox table (reliable delivery)
- Processed asynchronously by background job

### CQRS with MediatR (ADR 002)

**Commands** (Write Operations):
```csharp
public record CreateProductCommand(string Name, decimal Price) : ICommand<Guid>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Validate, create entity, save
        return Result.Ok(productId);
    }
}
```

**Queries** (Read Operations):
```csharp
public record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        // Read from database, map to DTO
        return Result.Ok(productDto);
    }
}
```

**Separation Benefits:**
- Optimize queries independently (read models, indexes)
- Different validation rules for reads vs writes
- Easier to cache query results

### Repository Pattern

**Interface in Core:**
```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<List<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Remove(Product product);
}
```

**Implementation in Infrastructure:**
```csharp
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct)
    {
        await _context.Products.AddAsync(product, ct);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }
}
```

### Unit of Work Pattern

**Interface:**
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<bool> SaveEntitiesAsync(CancellationToken ct = default);
}
```

**Implementation via DbContext:**
```csharp
public class CatalogDbContext : DbContext, IUnitOfWork
{
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(ct);

        return await base.SaveChangesAsync(ct);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken ct = default)
    {
        try
        {
            await SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}
```

### Naming Conventions

**C# Code:**
- Classes: `PascalCase`
- Methods: `PascalCase`
- Properties: `PascalCase`
- Private fields: `_camelCase`
- Constants: `PascalCase`
- Interfaces: `IPascalCase`

**Database:**
- Tables: `snake_case`
- Columns: `snake_case`
- Schemas: lowercase
- Constraints: `{table}_{column}_constraint_type`

**Files & Folders:**
- Project folders: `PascalCase`
- Namespaces match folder structure
- One class per file (except nested classes)
- File name matches primary class name

---

## Testing Guidelines

### Test Structure (Planned)

```
tests/
├── UnitTests/
│   ├── Users.Core.UnitTests/
│   ├── Catalog.Core.UnitTests/
│   └── ...
├── IntegrationTests/
│   ├── Users.Infrastructure.IntegrationTests/
│   ├── Catalog.Infrastructure.IntegrationTests/
│   └── ...
└── EndToEndTests/
    └── Bcommerce.Api.E2ETests/
```

### Unit Testing

**Test Domain Logic:**
```csharp
public class ProductTests
{
    [Fact]
    public void Create_ValidData_ReturnsProduct()
    {
        // Arrange
        var name = "Test Product";
        var price = 99.99m;

        // Act
        var product = Product.Create(name, price, 10);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
        product.DomainEvents.Should().ContainSingle(e => e is ProductCreatedEvent);
    }

    [Fact]
    public void UpdatePrice_NegativePrice_ThrowsDomainException()
    {
        // Arrange
        var product = Product.Create("Test", 100m, 10);

        // Act
        var act = () => product.UpdatePrice(-10m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*positive*");
    }
}
```

**Test Command Handlers:**
```csharp
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesProduct()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository, unitOfWork);

        var command = new CreateProductCommand("Product", 100m);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

### Integration Testing

**Test with Database:**
```csharp
public class ProductRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly CatalogDbContext _context;

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var product = Product.Create("Test", 100m, 10);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var repository = new ProductRepository(_context);

        // Act
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
    }
}
```

### API Testing

**Test Endpoints:**
```csharp
public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsOk()
    {
        // Arrange
        var productId = await CreateTestProduct();

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
    }
}
```

---

## AI Assistant Guidelines

### When Working with This Codebase

#### 1. **Understand Before Modifying**

- Always read the relevant domain entity before making changes
- Check existing patterns in the module you're working on
- Review ADRs (Architecture Decision Records) for context
- Look for similar implementations in other modules

#### 2. **Follow Established Patterns**

**DO:**
- Use CQRS for all use cases (Commands for writes, Queries for reads)
- Keep domain logic in entities, not in handlers
- Use Result pattern instead of throwing exceptions for expected failures
- Publish domain events for side effects
- Use integration events for cross-module communication
- Follow repository pattern for data access
- Use value objects for complex value types
- Apply specifications for complex queries

**DON'T:**
- Mix query and command logic
- Put infrastructure concerns in domain entities
- Use exceptions for control flow
- Directly reference other modules' entities
- Skip validation in command validators
- Bypass the repository pattern

#### 3. **Code Organization**

**File Naming:**
- Commands: `{Action}{Entity}Command.cs` (e.g., `CreateProductCommand.cs`)
- Handlers: `{Action}{Entity}CommandHandler.cs`
- Validators: `{Action}{Entity}CommandValidator.cs`
- Queries: `Get{Entity}By{Criteria}Query.cs`
- Events: `{Entity}{Action}Event.cs` (e.g., `ProductCreatedEvent.cs`)
- DTOs: `{Entity}Dto.cs`

**Folder Organization:**
- Group by feature, not by type
- Each command/query in its own folder
- Keep related files together

#### 4. **Database Considerations**

**When creating entities:**
- All aggregates have `Guid Id`
- Add `Version` for optimistic concurrency
- Implement `IAuditableEntity` for audit trails
- Implement `ISoftDeletable` for soft deletes
- Use proper value objects (Money, Address, etc.)
- Define all relationships and navigation properties

**When writing migrations:**
- Use descriptive migration names
- Keep migrations small and focused
- Test rollback scenarios
- Never modify applied migrations in production
- Use schema-specific migration history tables

#### 5. **Error Handling**

**Domain Layer:**
```csharp
// Use DomainException for invariant violations
if (quantity <= 0)
    throw new DomainException("Quantity must be positive");

// Or use Result pattern
if (quantity <= 0)
    return Result.Fail("Quantity must be positive");
```

**Application Layer:**
```csharp
// Return Result from handlers
public async Task<Result<ProductDto>> Handle(GetProductQuery request)
{
    var product = await _repository.GetByIdAsync(request.Id);
    if (product is null)
        return Result.Fail<ProductDto>(Error.NotFound("Product not found"));

    return Result.Ok(_mapper.Map<ProductDto>(product));
}
```

**Presentation Layer:**
```csharp
// Convert Result to IActionResult
var result = await Sender.Send(command);

return result.Match(
    onSuccess: value => Ok(value),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound => NotFound(error),
        ErrorType.Validation => BadRequest(error),
        ErrorType.Conflict => Conflict(error),
        _ => StatusCode(500, error)
    }
);
```

#### 6. **Performance Considerations**

- Use AsNoTracking() for read-only queries
- Implement pagination for list queries
- Add appropriate database indexes
- Consider caching for frequently accessed data
- Use Include() sparingly, prefer projection to DTOs
- Avoid N+1 queries

#### 7. **Security Considerations**

- Never expose entities directly in API responses (use DTOs)
- Validate all user input with FluentValidation
- Use authorization attributes on controllers
- Sanitize data before storing
- Hash sensitive data (passwords, tokens)
- Use parameterized queries (EF Core does this)

#### 8. **Documentation Standards**

**Code Comments:**
- XML comments on public APIs
- Inline comments for complex business logic
- No obvious comments (code should be self-documenting)

**README Files:**
- Update module README when adding features
- Document non-obvious design decisions
- Include usage examples

**API Documentation:**
- Keep OpenAPI spec updated
- Document request/response models
- Include example payloads

#### 9. **Common Tasks Reference**

**Add a new entity:**
1. Create entity in `{Module}.Core/Entities/`
2. Create repository interface in `{Module}.Core/Repositories/`
3. Create EF configuration in `{Module}.Infrastructure/Persistence/Configurations/`
4. Implement repository in `{Module}.Infrastructure/Repositories/`
5. Add DbSet to DbContext
6. Create migration

**Add a new command:**
1. Create command record in `{Module}.Application/Commands/{Feature}/`
2. Create validator
3. Create handler
4. Create request DTO in `{Module}.Presentation/Requests/`
5. Add controller endpoint

**Add a new query:**
1. Create query record in `{Module}.Application/Queries/{Feature}/`
2. Create response DTO in `{Module}.Application/DTOs/`
3. Create handler
4. Add controller endpoint

**Add integration event:**
1. Define event in `{Module}.Contracts/Events/`
2. Publish in domain event handler
3. Create handler in consuming module's Application layer
4. Register handler in DI

#### 10. **Debugging Tips**

**Common Issues:**

1. **Migration conflicts:**
   - Check which DbContext owns the table
   - Ensure correct schema in migration
   - Use `--context` flag explicitly

2. **Dependency injection errors:**
   - Verify service is registered in DI
   - Check service lifetime (Scoped, Transient, Singleton)
   - Ensure interface matches implementation

3. **Domain event not firing:**
   - Check if SaveChangesAsync is called
   - Verify DispatchDomainEventsAsync is implemented
   - Ensure handler is registered

4. **Optimistic concurrency conflicts:**
   - Check if Version is being incremented
   - Verify trigger is installed on table
   - Handle DbUpdateConcurrencyException

#### 11. **Before Submitting Code**

**Checklist:**
- [ ] Code compiles without errors
- [ ] Follows existing patterns in module
- [ ] Validation rules are complete
- [ ] Domain events published where appropriate
- [ ] Repository methods added if needed
- [ ] DTO created for API responses
- [ ] Controller endpoint added
- [ ] Migration created and tested
- [ ] No hardcoded values
- [ ] Error handling implemented
- [ ] Code is self-documenting or has comments

#### 12. **Reading the Codebase**

**Start here for different tasks:**

- **Understanding architecture:** `docs/architecture/README.md`, ADRs
- **Understanding a module:** Module's README, entity files
- **Understanding database:** `docs/db/schema.sql`
- **Understanding API:** `docs/api/endpoints.md`, Swagger UI
- **Understanding patterns:** `BuildingBlocks.Domain/README.md`
- **Understanding workflows:** This file (CLAUDE.md)

#### 13. **Useful Locations**

| What | Where |
|------|-------|
| ADRs | `docs/architecture/adr/` |
| Database schema | `docs/db/schema.sql` |
| Entity examples | `BuildingBlocks.Domain/README.md` |
| Implementation status | `IMPLEMENTATION_STATUS.md` |
| API contracts | `docs/api/` |
| Docker setup | `docker/docker-compose.yml` |
| Utility scripts | `script/` |

---

## Quick Reference

### Solution Structure
```
Bcommerce-Monolito.sln
├── Building Blocks (4 projects)
│   ├── BuildingBlocks.Domain          ✅ Complete
│   ├── BuildingBlocks.Application     🚧 Partial
│   ├── BuildingBlocks.Infrastructure  🚧 Configured
│   └── BuildingBlocks.Presentation    ✅ Complete
├── API (1 project)
│   └── Bcommerce.Api                  ✅ Configured
└── Modules
    └── Users (5 projects)             🚧 In Progress
        ├── Users.Core                 ✅ Complete
        ├── Users.Application          ⏳ Planned
        ├── Users.Infrastructure       🚧 Partial
        ├── Users.Contracts            ⏳ Planned
        └── Users.Presentation         ⏳ Planned
```

### Key Dependencies

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
```

### Environment Variables

```bash
# Database
DATABASE_HOST=localhost
DATABASE_PORT=5438
DATABASE_NAME=bcommerce_db
DATABASE_USER=bcommerce
DATABASE_PASSWORD=bcommerce

# API
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7000;http://localhost:5000
```

### Useful Commands Cheat Sheet

```bash
# Docker
./script/up.sh                  # Start PostgreSQL
./script/stop.sh                # Stop containers
./script/status.sh              # Check status

# Build
dotnet restore                  # Restore NuGet packages
dotnet build                    # Build solution
dotnet clean                    # Clean build artifacts

# Run
dotnet run --project src/api/Bcommerce.Api

# Database Migrations
dotnet ef migrations add <Name> \
  --project src/modules/users/Users.Infrastructure \
  --startup-project src/api/Bcommerce.Api \
  --context UsersDbContext

dotnet ef database update \
  --project src/modules/users/Users.Infrastructure \
  --startup-project src/api/Bcommerce.Api \
  --context UsersDbContext

# Tests (when available)
dotnet test                     # Run all tests
dotnet test --filter Category=Unit  # Run unit tests only
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial comprehensive documentation |

---

## Additional Resources

- **PRD:** `docs/PRD.md` - Complete product requirements
- **Architecture:** `docs/architecture/README.md` - Architecture overview
- **Database Schema:** `docs/db/schema.sql` - Complete database schema
- **API Docs:** `docs/api/endpoints.md` - API endpoint documentation
- **Implementation Status:** `IMPLEMENTATION_STATUS.md` - Current progress

---

**For questions or clarifications, refer to the ADRs in `docs/architecture/adr/` or consult the module-specific README files.**
