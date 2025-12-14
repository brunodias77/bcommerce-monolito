using BuildingBlocks.Infrastructure.Messaging.Integration;
using Cart.Core.Repositories;
using Microsoft.Extensions.Logging;
using Users.Contracts.Events;

namespace Cart.Application.IntegrationEventHandlers;

/// <summary>
/// Handler para o evento UserCreatedIntegrationEvent.
/// Cria um carrinho vazio para o novo usuário.
/// </summary>
/// <remarks>
/// Este handler é executado assincronamente pelo ProcessOutboxMessagesJob
/// quando o evento UserCreatedIntegrationEvent é processado.
/// </remarks>
public class UserCreatedIntegrationEventHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<UserCreatedIntegrationEventHandler> _logger;

    public UserCreatedIntegrationEventHandler(
        ICartRepository cartRepository,
        ILogger<UserCreatedIntegrationEventHandler> logger)
    {
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task HandleAsync(
        UserCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processando UserCreatedIntegrationEvent para usuário {UserId} - {Email}",
            @event.UserId,
            @event.Email);

        // Verifica se o usuário já tem um carrinho ativo
        var existingCart = await _cartRepository.UserHasActiveCartAsync(@event.UserId, cancellationToken);
        if (existingCart)
        {
            _logger.LogWarning(
                "Usuário {UserId} já possui um carrinho ativo. Ignorando criação.",
                @event.UserId);
            return;
        }

        // Cria um carrinho vazio para o novo usuário
        var cart = Core.Entities.Cart.CreateForUser(@event.UserId);

        await _cartRepository.AddAsync(cart, cancellationToken);
        await _cartRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Carrinho {CartId} criado com sucesso para usuário {UserId}",
            cart.Id,
            @event.UserId);
    }
}
