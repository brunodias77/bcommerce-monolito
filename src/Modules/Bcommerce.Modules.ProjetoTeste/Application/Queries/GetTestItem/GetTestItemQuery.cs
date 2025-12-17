
using Bcommerce.BuildingBlocks.Application.Models;
using Bcommerce.BuildingBlocks.Caching;
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;
using Bcommerce.Modules.ProjetoTeste.Domain.Repositories;
using MediatR;

namespace Bcommerce.Modules.ProjetoTeste.Application.Queries.GetTestItem;

public record GetTestItemQuery(Guid Id) : IRequest<Result<TestItemDto>>;

public record TestItemDto(Guid Id, string Name, decimal Value);

public class GetTestItemQueryHandler : IRequestHandler<GetTestItemQuery, Result<TestItemDto>>
{
    private readonly ITestItemRepository _repository;
    private readonly ICacheService _cacheService;

    public GetTestItemQueryHandler(ITestItemRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<Result<TestItemDto>> Handle(GetTestItemQuery request, CancellationToken cancellationToken)
    {
        // Example with Cache
        var cacheKey = $"testitem:{request.Id}";
        
        var cachedItem = await _cacheService.GetAsync<TestItemDto>(cacheKey, cancellationToken);
        if (cachedItem is not null) return Result<TestItemDto>.Success(cachedItem);

        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null) return Result<TestItemDto>.Failure(Error.NotFound("TestItem.NotFound", "Item not found"));

        var dto = new TestItemDto(item.Id, item.Name, item.Value);
        
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);

        return Result<TestItemDto>.Success(dto);
    }
}
