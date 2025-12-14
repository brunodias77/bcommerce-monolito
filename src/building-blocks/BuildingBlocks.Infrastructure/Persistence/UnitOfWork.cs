using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Implementação base do Unit of Work para EF Core.
/// </summary>
/// <remarks>
/// Esta é uma classe abstrata que pode ser herdada pelos DbContexts dos módulos
/// ou usada como mixin via extensão.
/// 
/// O EF Core já implementa o padrão Unit of Work internamente via ChangeTracker,
/// mas esta implementação adiciona:
/// - Suporte explícito a transações
/// - Método SaveEntitiesAsync para validação booleana
/// - Integração com Domain Events (via Interceptors)
/// 
/// Fluxo:
/// 1. Modificações são rastreadas pelo ChangeTracker
/// 2. SaveChangesAsync() persiste todas as mudanças
/// 3. Interceptors processam Domain Events
/// 4. Tudo acontece na mesma transação
/// </remarks>
public abstract class UnitOfWork : DbContext, IUnitOfWork
{
    protected UnitOfWork(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Salva todas as mudanças no banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de registros afetados</returns>
    /// <remarks>
    /// Este método:
    /// 1. Executa os Interceptors (AuditableEntity, SoftDelete, etc.)
    /// 2. Persiste as mudanças no banco
    /// 3. Faz commit da transação (se houver)
    /// 
    /// Os Interceptors fazem o trabalho pesado:
    /// - AuditableEntityInterceptor: CreatedAt, UpdatedAt
    /// - SoftDeleteInterceptor: Converte DELETE em UPDATE
    /// - OptimisticConcurrencyInterceptor: Incrementa Version
    /// - PublishDomainEventsInterceptor: Salva eventos no Outbox
    /// </remarks>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // O EF Core + Interceptors fazem todo o trabalho
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Salva todas as mudanças e retorna indicador de sucesso.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se salvou com sucesso, False caso contrário</returns>
    /// <remarks>
    /// Útil em cenários onde você precisa verificar se a operação foi bem-sucedida
    /// sem lançar exceção.
    /// 
    /// Exemplo:
    /// <code>
    /// var success = await _unitOfWork.SaveEntitiesAsync(ct);
    /// if (!success)
    /// {
    ///     return Result.Fail("Failed to save changes");
    /// }
    /// </code>
    /// </remarks>
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Conflito de concorrência (Optimistic Locking)
            return false;
        }
        catch (DbUpdateException)
        {
            // Erro de constraint do banco (FK, UNIQUE, etc.)
            return false;
        }
        catch
        {
            // Outros erros
            return false;
        }
    }
}

/// <summary>
/// Extensões para facilitar o uso do Unit of Work com DbContext.
/// </summary>
/// <remarks>
/// Se você não quiser herdar de UnitOfWork, pode usar estas extensões
/// para adicionar a funcionalidade a qualquer DbContext.
/// </remarks>
public static class UnitOfWorkExtensions
{
    /// <summary>
    /// Salva as mudanças e retorna indicador de sucesso.
    /// </summary>
    public static async Task<bool> SaveEntitiesAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Inicia uma transação explícita.
    /// </summary>
    /// <remarks>
    /// Use quando precisar de controle manual sobre transações.
    /// 
    /// Exemplo:
    /// <code>
    /// await using var transaction = await _context.BeginTransactionAsync(ct);
    /// try
    /// {
    ///     // Operações
    ///     await _context.SaveChangesAsync(ct);
    ///     await transaction.CommitAsync(ct);
    /// }
    /// catch
    /// {
    ///     await transaction.RollbackAsync(ct);
    ///     throw;
    /// }
    /// </code>
    /// </remarks>
    public static async Task<IDbContextTransaction> BeginTransactionAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se há mudanças pendentes no ChangeTracker.
    /// </summary>
    public static bool HasPendingChanges(this DbContext context)
    {
        return context.ChangeTracker.HasChanges();
    }

    /// <summary>
    /// Obtém o número de entidades rastreadas pelo ChangeTracker.
    /// </summary>
    public static int GetTrackedEntitiesCount(this DbContext context)
    {
        return context.ChangeTracker.Entries().Count();
    }

    /// <summary>
    /// Limpa o ChangeTracker (útil para testes ou cenários específicos).
    /// </summary>
    public static void ClearChangeTracker(this DbContext context)
    {
        context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Executa uma operação dentro de uma transação.
    /// </summary>
    /// <remarks>
    /// Abstração de alto nível para executar operações transacionais.
    /// 
    /// Exemplo:
    /// <code>
    /// var result = await _context.ExecuteInTransactionAsync(async () =>
    /// {
    ///     var order = Order.Create(...);
    ///     await _orderRepository.AddAsync(order);
    ///     await _context.SaveChangesAsync();
    ///     return order.Id;
    /// }, ct);
    /// </code>
    /// </remarks>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        this DbContext context,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// Executa uma operação dentro de uma transação (sem retorno).
    /// </summary>
    public static async Task ExecuteInTransactionAsync(
        this DbContext context,
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}

/// <summary>
/// Extensões para tratamento de erros do EF Core.
/// </summary>
public static class DbUpdateExceptionExtensions
{
    /// <summary>
    /// Verifica se a exceção é um conflito de concorrência.
    /// </summary>
    public static bool IsConcurrencyConflict(this Exception exception)
    {
        return exception is DbUpdateConcurrencyException;
    }

    /// <summary>
    /// Verifica se a exceção é uma violação de constraint.
    /// </summary>
    public static bool IsConstraintViolation(this Exception exception)
    {
        return exception is DbUpdateException;
    }

    /// <summary>
    /// Obtém mensagem amigável de erro de concorrência.
    /// </summary>
    public static string GetConcurrencyErrorMessage(this DbUpdateConcurrencyException exception)
    {
        var entry = exception.Entries.FirstOrDefault();
        if (entry != null)
        {
            var entityType = entry.Entity.GetType().Name;
            return $"The {entityType} was modified by another user. Please refresh and try again.";
        }

        return "A concurrency conflict occurred. Please refresh and try again.";
    }

    /// <summary>
    /// Obtém mensagem amigável de erro de constraint.
    /// </summary>
    public static string GetConstraintErrorMessage(this DbUpdateException exception)
    {
        var innerException = exception.InnerException?.Message ?? exception.Message;

        if (innerException.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            return "A record with this value already exists.";

        if (innerException.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
            return "This operation violates a data relationship constraint.";

        if (innerException.Contains("check constraint", StringComparison.OrdinalIgnoreCase))
            return "The data violates a validation rule.";

        if (innerException.Contains("null value", StringComparison.OrdinalIgnoreCase))
            return "A required field is missing.";

        return "A database constraint was violated.";
    }
}

/// <summary>
/// Exemplo de implementação em um módulo específico.
/// </summary>
public class ExampleModuleDbContext : UnitOfWork
{
    public ExampleModuleDbContext(DbContextOptions<ExampleModuleDbContext> options)
        : base(options)
    {
    }

    // DbSets
    // public DbSet<MyEntity> MyEntities => Set<MyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações específicas do módulo
        modelBuilder.HasDefaultSchema("example");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}

/// <summary>
/// Exemplo alternativo: DbContext que implementa IUnitOfWork diretamente.
/// </summary>
/// <remarks>
/// Use esta abordagem se preferir composição ao invés de herança.
/// </remarks>
public class AlternativeDbContext : DbContext, IUnitOfWork
{
    public AlternativeDbContext(DbContextOptions<AlternativeDbContext> options)
        : base(options)
    {
    }

    // Implementação explícita de IUnitOfWork
    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    async Task<bool> IUnitOfWork.SaveEntitiesAsync(CancellationToken cancellationToken)
    {
        return await this.SaveEntitiesAsync(cancellationToken);
    }
}

/// <summary>
/// Exemplos de uso do Unit of Work.
/// </summary>
public static class UsageExamples
{
    /*
    // ========================================
    // 1. USO BÁSICO
    // ========================================
    
    public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public async Task<Result<Guid>> Handle(
            CreateOrderCommand command,
            CancellationToken ct)
        {
            var order = Order.Create(command.UserId, command.Items);
            
            await _orderRepository.AddAsync(order, ct);
            
            // SaveChangesAsync() dispara os Interceptors:
            // - AuditableEntityInterceptor: CreatedAt, UpdatedAt
            // - PublishDomainEventsInterceptor: Salva eventos no Outbox
            await _unitOfWork.SaveChangesAsync(ct);
            
            return Result.Ok(order.Id);
        }
    }

    // ========================================
    // 2. COM VALIDAÇÃO DE SUCESSO
    // ========================================
    
    public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public async Task<Result> Handle(
            UpdateProductCommand command,
            CancellationToken ct)
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
            
            if (product == null)
                return Result.Fail(Error.NotFound("PRODUCT_NOT_FOUND", "Product not found"));
            
            product.UpdatePrice(command.NewPrice);
            
            // Tenta salvar e retorna false se falhar (ex: concorrência)
            var success = await _unitOfWork.SaveEntitiesAsync(ct);
            
            if (!success)
                return Result.Fail(Error.Conflict("SAVE_FAILED", "Failed to save changes"));
            
            return Result.Ok();
        }
    }

    // ========================================
    // 3. TRANSAÇÃO EXPLÍCITA (Cenário avançado)
    // ========================================
    
    public class ComplexOperationHandler
    {
        private readonly OrdersDbContext _ordersContext;
        private readonly PaymentsDbContext _paymentsContext;

        public async Task<Result> Handle(CancellationToken ct)
        {
            // ATENÇÃO: Transações entre múltiplos DbContexts são complexas!
            // Prefira Integration Events (Outbox Pattern) para comunicação entre módulos.
            
            await using var transaction = await _ordersContext.BeginTransactionAsync(ct);
            try
            {
                // Operação 1
                var order = Order.Create(...);
                await _ordersContext.SaveChangesAsync(ct);
                
                // Operação 2
                var payment = Payment.Create(...);
                await _paymentsContext.SaveChangesAsync(ct);
                
                // Commit
                await transaction.CommitAsync(ct);
                return Result.Ok();
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    // ========================================
    // 4. USO COM EXTENSÃO ExecuteInTransactionAsync
    // ========================================
    
    public class OrderService
    {
        private readonly OrdersDbContext _context;

        public async Task<Guid> CreateOrderWithTransaction(
            CreateOrderRequest request,
            CancellationToken ct)
        {
            return await _context.ExecuteInTransactionAsync(async () =>
            {
                // Operação 1
                var order = Order.Create(request.UserId, request.Items);
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(ct);
                
                // Operação 2
                var notification = Notification.Create(order.UserId, "Order created");
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync(ct);
                
                return order.Id;
            }, ct);
        }
    }

    // ========================================
    // 5. TRATAMENTO DE ERROS
    // ========================================
    
    public class SafeUpdateHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public async Task<Result> Update(CancellationToken ct)
        {
            try
            {
                // Operações...
                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict");
                
                return Result.Fail(Error.Conflict(
                    "CONCURRENCY_CONFLICT",
                    ex.GetConcurrencyErrorMessage()
                ));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database constraint violation");
                
                return Result.Fail(Error.Conflict(
                    "CONSTRAINT_VIOLATION",
                    ex.GetConstraintErrorMessage()
                ));
            }
        }
    }

    // ========================================
    // 6. VERIFICAÇÃO DE MUDANÇAS PENDENTES
    // ========================================
    
    public class ChangeTrackerExample
    {
        private readonly DbContext _context;

        public async Task DoSomething()
        {
            // Verificar se há mudanças
            if (_context.HasPendingChanges())
            {
                Console.WriteLine($"Tracked entities: {_context.GetTrackedEntitiesCount()}");
                await _context.SaveChangesAsync();
            }
            
            // Limpar ChangeTracker (útil em cenários específicos)
            _context.ClearChangeTracker();
        }
    }
    */
}