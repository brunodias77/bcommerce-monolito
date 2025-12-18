using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Payments.Domain.Entities;

namespace Bcommerce.Modules.Payments.Domain.Repositories;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    // Additional methods if needed
}
