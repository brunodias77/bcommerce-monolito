using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Payments.Domain.Entities;
using Bcommerce.Modules.Payments.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Payments.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment, PaymentsDbContext>, IPaymentRepository
{
    public PaymentRepository(PaymentsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Payments
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }
}
