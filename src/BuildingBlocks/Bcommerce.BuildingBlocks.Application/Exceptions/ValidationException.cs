using Bcommerce.BuildingBlocks.Application.Models;
using FluentValidation.Results;

namespace Bcommerce.BuildingBlocks.Application.Exceptions;

public class ValidationException : ApplicationException
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Erro de Validação", "Um ou mais erros de validação ocorreram.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}
