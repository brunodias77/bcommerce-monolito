using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Payments.Domain.Entities;
using Bcommerce.Modules.Payments.Domain.Repositories;

namespace Bcommerce.Modules.Payments.Infrastructure.Persistence.Repositories;

public class PaymentMethodRepository : Repository<PaymentMethod, PaymentsDbContext>, IPaymentMethodRepository
{
    public PaymentMethodRepository(PaymentsDbContext dbContext) : base(dbContext)
    {
    }
}
