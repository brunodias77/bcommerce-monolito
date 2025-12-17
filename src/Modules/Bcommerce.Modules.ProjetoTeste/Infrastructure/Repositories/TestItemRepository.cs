
using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;
using Bcommerce.Modules.ProjetoTeste.Domain.Repositories;

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure.Repositories;

public class TestItemRepository : Repository<TestItem>, ITestItemRepository
{
    public TestItemRepository(TestDbContext context) : base(context)
    {
    }
}
