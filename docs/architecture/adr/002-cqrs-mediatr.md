# ADR 002: Adoção de CQRS com MediatR

## Status

**Aceito** - Dezembro 2024

## Contexto

Precisamos de um padrão para estruturar a lógica de aplicação que:
- Separe operações de leitura e escrita
- Facilite testes unitários
- Permita cross-cutting concerns (logging, validation, transactions)
- Seja consistente entre todos os módulos

## Decisão

Adotamos **CQRS (Command Query Responsibility Segregation)** implementado com **MediatR**.

### Estrutura

```
Application/
├── Commands/
│   ├── CreateProduct/
│   │   ├── CreateProductCommand.cs
│   │   ├── CreateProductCommandHandler.cs
│   │   └── CreateProductCommandValidator.cs
│   └── ...
├── Queries/
│   ├── GetProductById/
│   │   ├── GetProductByIdQuery.cs
│   │   └── GetProductByIdQueryHandler.cs
│   └── ...
└── DTOs/
    └── ProductDto.cs
```

### Interfaces Base (BuildingBlocks.Application)

```csharp
// Commands (escrita)
public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

// Queries (leitura)
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
```

### Pipeline Behaviors

1. **LoggingBehavior**: Loga entrada/saída de requests
2. **ValidationBehavior**: Valida com FluentValidation
3. **TransactionBehavior**: Gerencia transações para Commands

## Consequências

### Positivas
- ✅ Separação clara entre reads e writes
- ✅ Facilita otimização de queries (read models separados)
- ✅ Cross-cutting concerns centralizados
- ✅ Testes isolados por handler

### Negativas
- ❌ Mais arquivos por feature
- ❌ Curva de aprendizado para novos devs
- ❌ Overhead para operações muito simples

## Referências

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
