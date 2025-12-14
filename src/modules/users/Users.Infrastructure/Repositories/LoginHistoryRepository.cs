using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de histórico de login.
/// </summary>
internal class LoginHistoryRepository : ILoginHistoryRepository
{
    private readonly UsersDbContext _context;

    public LoginHistoryRepository(UsersDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<IReadOnlyList<LoginHistory>> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.LoginHistories
            .Where(lh => lh.UserId == userId)
            .OrderByDescending(lh => lh.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoginHistory>> GetRecentFailedByUserIdAsync(
        Guid userId,
        int minutes,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
        return await _context.LoginHistories
            .Where(lh => lh.UserId == userId && !lh.Success && lh.CreatedAt >= cutoffTime)
            .OrderByDescending(lh => lh.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default)
    {
        await _context.LoginHistories.AddAsync(loginHistory, cancellationToken);
    }
}