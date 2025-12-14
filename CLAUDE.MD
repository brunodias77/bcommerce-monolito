# CLAUDE.MD - Bcommerce Modular Monolith

## Project Overview

**Bcommerce-Monolito** is an e-commerce platform built as a modular monolith using .NET 8+, PostgreSQL, and Domain-Driven Design (DDD) architecture. The system implements logical separation into 6 main domain modules while maintaining referential integrity and enabling future evolution to microservices.

**Current Branch**: elastic-zhukovsky
**Main Branch**: main
**Solution**: Bcommerce-Monolito.sln

## Architecture

### Pattern
- Modular Monolith with DDD
- CQRS with MediatR
- Repository and Unit of Work patterns
- Outbox/Inbox pattern for inter-module communication
- Specification pattern for complex queries

### Technology Stack

**Backend**
- .NET 8+ / ASP.NET Core
- Entity Framework Core 8+
- ASP.NET Core Identity

**Database**
- PostgreSQL 15+ with extensions:
  - uuid-ossp (UUID generation)
  - citext (case-insensitive text)
  - pg_trgm (text similarity search)

**Infrastructure**
- MediatR 12.2.0 (CQRS mediator)
- FluentValidation 11.9.0
- AutoMapper 12.0.1
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0
- Redis (caching)
- ElasticSearch (catalog search)

## Project Structure

```
Bcommerce-Monolito/
├── src/
│   ├── building-blocks/          # Shared building blocks
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   └── BuildingBlocks.Presentation/
│   ├── modules/                  # Domain modules
│   │   └── users/               # User management module
│   │       ├── Users.Core/
│   │       ├── Users.Infrastructure/
│   │       ├── Users.Contracts/
│   │       └── Users.Presentation/
│   └── api/                     # API entry point
│       └── Bcommerce.Api/
├── docs/                        # Documentation
│   ├── PRD.md                   # Product Requirements Document
│   ├── tasks.md                 # Task tracking
│   ├── architecture/            # Architecture docs
│   ├── api/                     # API documentation
│   └── db/                      # Database documentation
├── docker/                      # Docker configurations
├── script/                      # Utility scripts
└── IMPLEMENTATION_STATUS.md     # Current implementation status
```

## Domain Modules

### Planned Modules (6)

1. **Users** - User management, authentication, profiles, notifications
2. **Catalog** - Products, categories, inventory, pricing
3. **Orders** - Shopping cart, checkout, order processing
4. **Payments** - Payment processing, transactions, refunds
5. **Shipping** - Delivery management, tracking
6. **Reviews** - Product reviews and ratings

### Module Layer Structure

Each module follows Clean Architecture with 4 layers:

- **Core**: Domain layer (Entities, Value Objects, Events, Repository interfaces)
- **Application**: Use cases (Commands, Queries, DTOs, Services)
- **Infrastructure**: Implementations (Persistence, External Services, Caching)
- **Contracts**: Integration events for inter-module communication

## Building Blocks Status

### BuildingBlocks.Domain (✅ 100% Complete)

**Entities (4 files)**
- Entity.cs - Base entity with domain events
- AggregateRoot.cs - Aggregate root with versioning
- IAuditableEntity.cs - Auditing interface
- ISoftDeletable.cs - Soft delete interface

**Value Objects (5 files)**
- ValueObject.cs - Base class
- Money.cs - Money value object with operators
- Address.cs - Generic address value object
- DateRange.cs - Date range value object
- Enumeration.cs - Smart enum base class

**Events (4 files)**
- IDomainEvent.cs
- DomainEvent.cs
- DomainEventBase.cs
- IEventHandler.cs

**Repositories (4 files)**
- IRepository.cs - Generic repository
- IReadRepository.cs - Read-only repository
- IWriteRepository.cs - Write-only repository
- IUnitOfWork.cs

**Specifications (6 files)**
- ISpecification.cs
- Specification.cs
- AndSpecification.cs
- OrSpecification.cs
- NotSpecification.cs
- SpecificationExtensions.cs

**Exceptions (4 files)**
- DomainException.cs
- EntityNotFoundException.cs
- BusinessRuleValidationException.cs
- InvalidValueObjectException.cs

**Business Rules (3 files)**
- IBusinessRule.cs
- BusinessRule.cs
- BusinessRuleValidator.cs

### BuildingBlocks.Application (🚧 14% Complete)

**Results (✅ 5/5 files)**
- ErrorType.cs
- Error.cs
- Result.cs
- Result<T>.cs
- ResultExtensions.cs

**Pending Implementation (32 files)**
- Commands (5 files) - ICommand, ICommandHandler, CommandBase
- Queries (4 files) - IQuery, IQueryHandler, QueryBase
- Behaviors (6 files) - Validation, Logging, Transaction, Performance, Caching, Authorization
- Validators & Mappings (6 files)
- DTOs (2 files)
- Exceptions (5 files)
- Pagination (4 files)

### BuildingBlocks.Infrastructure (⏳ 0% Complete)

**Pending Implementation (34 files)**
- Persistence (5 files) - ModuleDbContext, RepositoryBase
- Interceptors (4 files) - Auditing, Soft Delete, Domain Events
- Outbox Pattern (7 files)
- Inbox Pattern (7 files)
- Idempotency (6 files)
- UnitOfWork & Transactions (5 files)

## Current Implementation Status

**Overall Progress**: 37% complete (36/97 files)

| Layer | Complete | Total | Progress |
|-------|----------|-------|----------|
| Domain | 31/31 | 31 | 100% ✅ |
| Application | 5/37 | 37 | 14% 🚧 |
| Infrastructure | 0/34 | 34 | 0% ⏳ |

See IMPLEMENTATION_STATUS.md for detailed breakdown.

## Development Guidelines

### Key Principles

1. **Domain-Driven Design**: Follow DDD tactical patterns
2. **CQRS**: Separate read and write operations
3. **Repository Pattern**: Abstract data access
4. **Specification Pattern**: Compose complex queries
5. **Result Pattern**: Use Result<T> instead of exceptions for business errors
6. **Event-Driven**: Use domain events for side effects

### Coding Conventions

- Use C# 12 features and nullable reference types
- Follow Clean Code principles
- Implement proper validation with FluentValidation
- Use AutoMapper for DTO mapping
- All business logic in domain layer
- Keep controllers thin (delegate to MediatR)

### Database

- PostgreSQL schemas for module separation
- EF Core migrations per module
- Optimistic concurrency with row versioning
- Soft delete for historical data
- Audit trail with timestamps and user tracking

### Testing

- Unit tests for domain logic
- Integration tests for repositories
- API tests for endpoints
- Follow AAA pattern (Arrange, Act, Assert)

## Common Commands

### Build
```bash
dotnet build Bcommerce-Monolito.sln
dotnet build src/building-blocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj
```

### Restore Dependencies
```bash
dotnet restore
```

### Run API
```bash
dotnet run --project src/api/Bcommerce.Api/Bcommerce.Api.csproj
```

### Migrations (per module)
```bash
dotnet ef migrations add <MigrationName> --project src/modules/users/Users.Infrastructure
dotnet ef database update --project src/modules/users/Users.Infrastructure
```

## Important Files

- **PRD.md** - Complete product requirements and specifications
- **IMPLEMENTATION_STATUS.md** - Detailed implementation progress
- **docs/architecture/** - Architecture documentation and diagrams
- **docs/tasks.md** - Task tracking and planning
- **.gitignore** - Git ignore configuration

## Next Priority Tasks

### High Priority
1. Implement Commands and Queries (Application layer)
2. Implement Pagination support
3. Implement Application exceptions

### Medium Priority
4. Implement Persistence layer (ModuleDbContext, RepositoryBase)
5. Implement EF Core Interceptors (Auditing, Soft Delete)
6. Implement MediatR Behaviors (Validation, Logging, Transaction)

### Low Priority
7. Implement Outbox/Inbox patterns
8. Implement Idempotency support

## Known Issues

- BuildingBlocks.Domain has nullable warnings (CS8618) - can be safely ignored or fixed
- Application and Infrastructure layers need completion before module implementation
- ElasticSearch integration not yet implemented

## Integration Points

### Module Communication
- Use integration events via Contracts projects
- Events processed through Outbox/Inbox pattern
- Maintain eventual consistency between modules

### External Services
- Payment gateways (PIX, Boleto, Credit Cards)
- Email/SMS notifications
- CEP validation services
- Shipping carriers APIs

## Resources

- Documentation: /docs/
- PRD: /docs/PRD.md
- Architecture: /docs/architecture/
- API Specs: /docs/api/

## Notes for Claude

- This is a modular monolith, not microservices
- Each module has its own PostgreSQL schema
- Use Result pattern for operation outcomes
- Domain events are key for module communication
- Focus on completing Building Blocks before module implementation
- Follow DDD patterns strictly
- Keep modules independent and loosely coupled
