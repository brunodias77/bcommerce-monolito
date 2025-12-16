using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Results;
using BuildingBlocks.Infrastructure.Messaging.Integration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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
        [FromKeyedServices("users")] IEventBus eventBus,
        IEmailService emailService,
        IProfileRepository profileRepository,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _emailService = emailService;
        _profileRepository = profileRepository;
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
                "⚠️ [users] Tentativa de registro com email já existente: {Email}",
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
                "❌ [users] Falha ao criar usuário {Email}: {Errors}",
                command.Email,
                errors);

            return Result.Fail<Guid>(Error.Failure(
                "USER_CREATION_FAILED",
                $"Falha ao criar usuário: {errors}"));
        }

        _logger.LogInformation(
            "✅ [users] Usuário criado com sucesso: {UserId} - {Email}",
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
                "✅ [users] Perfil criado para usuário {UserId}: {FirstName} {LastName}",
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
            "📤 [users] UserCreatedIntegrationEvent publicado para módulo Cart: {UserId}",
            user.Id);

        var displayName = !string.IsNullOrEmpty(command.FirstName) 
            ? command.FirstName 
            : command.Email.Split('@')[0];

        // ========================================
        // Passo 7: Notificação - Email de Confirmação
        // ========================================
        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var confirmationLink = $"http://localhost:5020/api/users/confirm-email?userId={user.Id}&token={encodedToken}";

            // TODO: Em produção, remover este log e deixar apenas o envio de email
            _logger.LogWarning("📧 [users] Link de confirmação para {Email}: {ConfirmationLink}", command.Email, confirmationLink);

            await _emailService.SendEmailConfirmationAsync(
                command.Email,
                displayName,
                confirmationLink,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "⚠️ [users] Falha ao enviar email de confirmação para {Email}",
                command.Email);
        }

        // ========================================
        // Passo 8: Retorno do ID
        // ========================================
        return Result.Ok(user.Id);
    }
}

