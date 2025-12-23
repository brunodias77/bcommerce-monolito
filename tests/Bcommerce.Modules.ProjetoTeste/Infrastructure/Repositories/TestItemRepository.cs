
using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;
using Bcommerce.Modules.ProjetoTeste.Application.Repositories;
using Bcommerce.Modules.ProjetoTeste.Infrastructure.Data;

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure.Repositories;

public class TestItemRepository : Repository<TestItem, TestDbContext>, ITestItemRepository
{
    public TestItemRepository(TestDbContext context) : base(context)
    {
    }
}
