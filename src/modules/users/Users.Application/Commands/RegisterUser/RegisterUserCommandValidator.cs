using FluentValidation;

namespace Users.Application.Commands.RegisterUser;

/// <summary>
/// Validador para RegisterUserCommand.
/// Implementa as regras RN-01 e RN-03 da especificação.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // RN-01: Validação de Email (formato RFC 5322)
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("O e-mail é obrigatório.")
                .WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress()
                .WithMessage("O e-mail informado não possui um formato válido.")
                .WithErrorCode("EMAIL_INVALID_FORMAT")
            .MaximumLength(256)
                .WithMessage("O e-mail não pode exceder 256 caracteres.")
                .WithErrorCode("EMAIL_TOO_LONG");

        // RN-03: Complexidade de Senha
        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("A senha é obrigatória.")
                .WithErrorCode("PASSWORD_REQUIRED")
            .MinimumLength(8)
                .WithMessage("A senha deve conter no mínimo 8 caracteres.")
                .WithErrorCode("PASSWORD_TOO_SHORT")
            .Matches("[A-Z]")
                .WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
                .WithErrorCode("PASSWORD_MISSING_UPPERCASE")
            .Matches("[0-9]")
                .WithMessage("A senha deve conter pelo menos um número.")
                .WithErrorCode("PASSWORD_MISSING_NUMBER");

        // FirstName e LastName são opcionais, mas se fornecidos devem ter tamanho válido
        When(x => !string.IsNullOrEmpty(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                    .WithMessage("O primeiro nome não pode exceder 100 caracteres.")
                    .WithErrorCode("FIRSTNAME_TOO_LONG");
        });

        When(x => !string.IsNullOrEmpty(x.LastName), () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(100)
                    .WithMessage("O sobrenome não pode exceder 100 caracteres.")
                    .WithErrorCode("LASTNAME_TOO_LONG");
        });
    }
}
