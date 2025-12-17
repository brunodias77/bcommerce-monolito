
using Bcommerce.BuildingBlocks.Application.Abstractions.Data; // IRepository location
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;

namespace Bcommerce.Modules.ProjetoTeste.Domain.Repositories;

public interface ITestItemRepository : IRepository<TestItem>
{
    // Custom methods if needed
}
