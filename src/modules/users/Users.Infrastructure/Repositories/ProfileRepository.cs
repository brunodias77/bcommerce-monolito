using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de perfis.
/// </summary>
internal class ProfileRepository : IProfileRepository
{
    private readonly UsersDbContext _context;

    public ProfileRepository(UsersDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Profile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<Profile?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.Cpf == cpf, cancellationToken);
    }

    public async Task<bool> CpfExistsAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Profiles
            .AnyAsync(p => p.Cpf == cpf, cancellationToken);
    }

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        await _context.Profiles.AddAsync(profile, cancellationToken);
    }

    public void Update(Profile profile)
    {
        _context.Profiles.Update(profile);
    }

    public void Remove(Profile profile)
    {
        profile.Delete();
        _context.Profiles.Update(profile);
    }
}
