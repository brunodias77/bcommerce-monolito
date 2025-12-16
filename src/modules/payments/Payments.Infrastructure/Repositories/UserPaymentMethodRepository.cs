using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Entities;
using Payments.Core.Repositories;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Repositories;

public class UserPaymentMethodRepository : IUserPaymentMethodRepository
{
    private readonly PaymentsDbContext _context;

    public UserPaymentMethodRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<UserPaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.UserPaymentMethods
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UserPaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.UserPaymentMethods
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserPaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.UserPaymentMethods
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IsDefault, cancellationToken);
    }

    public async Task AddAsync(UserPaymentMethod method, CancellationToken cancellationToken)
    {
        await _context.UserPaymentMethods.AddAsync(method, cancellationToken);
    }

    public Task UpdateAsync(UserPaymentMethod method, CancellationToken cancellationToken)
    {
        _context.UserPaymentMethods.Update(method);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserPaymentMethod method, CancellationToken cancellationToken)
    {
        _context.UserPaymentMethods.Remove(method);
        return Task.CompletedTask;
    }
}
