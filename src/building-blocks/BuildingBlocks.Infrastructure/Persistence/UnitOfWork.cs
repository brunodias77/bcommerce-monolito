using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Implementação base do padrão Unit of Work que encapsula o <see cref="DbContext"/>.
/// </summary>
/// <remarks>
/// <strong>Propósito Arquitetural:</strong>
/// O padrão Unit of Work mantém uma lista de objetos afetados por uma transação de negócio e coordena a escrita de
/// mudanças e a resolução de problemas de concorrência.
/// 
/// Nesta arquitetura, o próprio <see cref="DbContext"/> já implementa o padrão UnitOfWork e Repository nativamente.
/// Esta classe serve principalmente para:
/// 1. Adaptar o <see cref="DbContext"/> para a interface <see cref="IUnitOfWork"/> definida na camada de Domínio (Inversão de Dependência).
/// 2. Centralizar lógica de transações explícitas (<see cref="BeginTransactionAsync"/>).
/// 
/// <strong>Estratégias de Uso:</strong>
/// - <strong>Herança:</strong> Seu DbContext herda de <see cref="UnitOfWork"/>.
/// - <strong>Composição:</strong> Se herdar de IdentityDbContext, implemente <see cref="IUnitOfWork"/> manualmente ou use extensões.
/// </remarks>
public class UnitOfWork : DbContext, IUnitOfWork
{
    public UnitOfWork(DbContextOptions options) : base(options)
    {
    }

    protected UnitOfWork() : base()
    {
    }

    /// <summary>
    /// Salva todas as mudanças pendentes.
    /// </summary>
    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Salva todas as mudanças e retorna indicador de sucesso.
    /// </summary>
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Inicia uma nova transação.
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se há uma transação ativa.
    /// </summary>
    public bool HasActiveTransaction => Database.CurrentTransaction != null;

    /// <summary>
    /// Obtém a transação atual.
    /// </summary>
    public IDbContextTransaction? CurrentTransaction => Database.CurrentTransaction;
}
