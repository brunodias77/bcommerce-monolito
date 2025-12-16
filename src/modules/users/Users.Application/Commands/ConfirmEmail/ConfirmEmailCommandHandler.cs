using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Results;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Integration;
using Microsoft.AspNetCore.Identity;
using Users.Core.Entities;
using Users.Contracts.Events;

namespace Users.Application.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ConfirmEmailCommandHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        // 2. Recuperação do Usuário
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new EntityNotFoundException(nameof(User), request.UserId);
        }

        // RN-03 (Idempotência)
        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            return Result.Ok();
        }

        // 3. Processamento de Confirmação (Identity)
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        // 4. Verificação de Resultado
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            // Retornando Result.Failure conforme padrão, mapeando para 400 no controller
            return Result.Fail(Error.Validation("Identity.ConfirmEmailFailed", errors));
        }

        // 5. Geração de Evento
        var integrationEvent = new UserEmailConfirmedIntegrationEvent(user.Id, user.Email!);
        
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
        
        // 6. Persistência (Unit of Work)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
