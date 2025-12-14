using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Results;
using BuildingBlocks.Infrastructure.Messaging.Integration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Users.Contracts.Events;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Services;

namespace Users.Application.Commands.RegisterUser;

/// <summary>
/// Handler para o comando de registro de usuário.
/// Implementa o fluxo de processamento conforme especificação.
/// </summary>
/// <remarks>
/// Fluxo:
/// 1. Verificação de existência (email único - RN-02)
/// 2. Criação do usuário via UserManager (hash de senha - RN-04)
/// 3. Criação do perfil (RN-05)
/// 4. Publicação do Integration Event (RN-06 - Cart)
/// 5. Envio de email de boas-vindas
/// 6. Retorno do ID do usuário criado
/// </remarks>
internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly UserManager<User> _userManager;
    private readonly IProfileRepository _profileRepository;
    private readonly IEventBus _eventBus;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        UserManager<User> userManager,
        IProfileRepository profileRepository,
        IEventBus eventBus,
        IEmailService emailService,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userManager = userManager;
        _profileRepository = profileRepository;
        _eventBus = eventBus;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        RegisterUserCommand command, 
        CancellationToken cancellationToken)
    {
        // ========================================
        // Passo 2: Verificação de Existência (RN-02)
        // ========================================
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
        {
            _logger.LogWarning(
                "Tentativa de registro com email já existente: {Email}",
                command.Email);

            return Result.Fail<Guid>(Error.Conflict(
                "EMAIL_ALREADY_EXISTS",
                "O e-mail informado já está cadastrado no sistema."));
        }

        // ========================================
        // Passo 3 & 4: Criação do Usuário (RN-04 - hash automático pelo Identity)
        // ========================================
        var user = new User();
        user.Email = command.Email;
        user.UserName = command.Email; // Usando email como username
        user.EmailConfirmed = false;
        user.LockoutEnabled = true;

        // UserManager.CreateAsync já faz:
        // - Normalização de email/username
        // - Hash da senha com BCrypt (Identity default)
        // - Validação de senha (via PasswordValidator)
        // - Persistência no banco
        var createResult = await _userManager.CreateAsync(user, command.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogError(
                "Falha ao criar usuário {Email}: {Errors}",
                command.Email,
                errors);

            return Result.Fail<Guid>(Error.Failure(
                "USER_CREATION_FAILED",
                $"Falha ao criar usuário: {errors}"));
        }

        _logger.LogInformation(
            "Usuário criado com sucesso: {UserId} - {Email}",
            user.Id,
            command.Email);

        // ========================================
        // Passo 5: Criação do Perfil (RN-05)
        // ========================================
        if (!string.IsNullOrEmpty(command.FirstName) || !string.IsNullOrEmpty(command.LastName))
        {
            var profile = new Profile(
                user.Id,
                command.FirstName ?? "",
                command.LastName ?? "");

            await _profileRepository.AddAsync(profile, cancellationToken);
            await _profileRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Perfil criado para usuário {UserId}: {FirstName} {LastName}",
                user.Id,
                command.FirstName,
                command.LastName);
        }

        // ========================================
        // Passo 6: Publicação do Integration Event (RN-06)
        // ========================================
        var integrationEvent = new UserCreatedIntegrationEvent(
            user.Id,
            command.Email,
            command.Email, // UserName
            command.FirstName,
            command.LastName,
            DateTime.UtcNow);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "UserCreatedIntegrationEvent publicado para módulo Cart: {UserId}",
            user.Id);

        // ========================================
        // Passo 7: Notificação - Email de Boas-Vindas
        // ========================================
        try
        {
            var displayName = !string.IsNullOrEmpty(command.FirstName) 
                ? command.FirstName 
                : command.Email.Split('@')[0];

            await _emailService.SendWelcomeEmailAsync(
                command.Email,
                displayName,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Não falha o registro se o email não for enviado
            _logger.LogWarning(
                ex,
                "Falha ao enviar email de boas-vindas para {Email}",
                command.Email);
        }

        // ========================================
        // Passo 8: Retorno do ID
        // ========================================
        return Result.Ok(user.Id);
    }
}

