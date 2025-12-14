using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de sessões.
/// </summary>
internal class SessionRepository : ISessionRepository
{
    private readonly UsersDbContext _context;

    public SessionRepository(UsersDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Session?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<Session>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.Sessions
            .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > now)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetCurrentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsCurrent, cancellationToken);
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        await _context.Sessions.AddAsync(session, cancellationToken);
    }

    public void Update(Session session)
    {
        _context.Sessions.Update(session);
    }

    public void Remove(Session session)
    {
        _context.Sessions.Remove(session);
    }
}