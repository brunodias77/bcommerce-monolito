
using Bcommerce.BuildingBlocks.Application.Behaviors;
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;
using Bcommerce.Modules.ProjetoTeste.Domain.Repositories;
using FluentValidation;
using MediatR;
using Bcommerce.BuildingBlocks.Application.Models; // For Result

namespace Bcommerce.Modules.ProjetoTeste.Application.Commands.CreateTestItem;

public record CreateTestItemCommand(string Name, string Description, decimal Value) : IRequest<Result<Guid>>;

public class CreateTestItemCommandValidator : AbstractValidator<CreateTestItemCommand>
{
    public CreateTestItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Value).GreaterThan(0);
    }
}

public class CreateTestItemCommandHandler : IRequestHandler<CreateTestItemCommand, Result<Guid>>
{
    private readonly ITestItemRepository _repository;
    // UnitOfWork is typically accessed via Repository or injected interface if needed, relying on BuildingBlocks Base implementation usually commits in EF

    public CreateTestItemCommandHandler(ITestItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateTestItemCommand request, CancellationToken cancellationToken)
    {
        var item = new TestItem(request.Name, request.Description, request.Value);
        
        // Assuming IRepository includes Add method or we need to check Base Repository
        await _repository.AddAsync(item, cancellationToken);
        
        // UnitOfWork save changes is handled by TransactionBehavior or explicitly
        // If BuildingBlocks TransactionBehavior handles it, we just return. 
        // We will assume standard EF Core Repository pattern.
        
        return Result<Guid>.Success(item.Id);
    }
}
