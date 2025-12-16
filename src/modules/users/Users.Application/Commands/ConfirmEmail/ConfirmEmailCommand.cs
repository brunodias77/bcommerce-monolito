using BuildingBlocks.Application.Abstractions;
using FluentValidation;

namespace Users.Application.Commands.ConfirmEmail;

public record ConfirmEmailCommand(Guid UserId, string Token) : ICommand;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
