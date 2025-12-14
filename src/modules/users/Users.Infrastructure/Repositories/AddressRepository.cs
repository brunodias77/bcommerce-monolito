using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de endereços.
/// </summary>
internal class AddressRepository : IAddressRepository
{
    private readonly UsersDbContext _context;

    public AddressRepository(UsersDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, cancellationToken);
    }

    public async Task AddAsync(Address address, CancellationToken cancellationToken = default)
    {
        await _context.Addresses.AddAsync(address, cancellationToken);
    }

    public void Update(Address address)
    {
        _context.Addresses.Update(address);
    }

    public void Remove(Address address)
    {
        address.Delete();
        _context.Addresses.Update(address);
    }
}