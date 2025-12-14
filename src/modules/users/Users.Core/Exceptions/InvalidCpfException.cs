using BuildingBlocks.Domain.Exceptions;

namespace Users.Core.Exceptions;

/// <summary>
/// Exception thrown when a CPF is invalid.
/// </summary>
public sealed class InvalidCpfException : BusinessRuleValidationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidCpfException"/> class.
    /// </summary>
    /// <param name="cpf">The invalid CPF value</param>
    public InvalidCpfException(string cpf)
        : base($"CPF '{cpf}' is invalid.")
    {
        Cpf = cpf;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidCpfException"/> class with a specific reason.
    /// </summary>
    /// <param name="cpf">The invalid CPF value</param>
    /// <param name="reason">The specific reason why the CPF is invalid</param>
    public InvalidCpfException(string cpf, string reason)
        : base($"CPF '{cpf}' is invalid: {reason}")
    {
        Cpf = cpf;
        Reason = reason;
    }

    /// <summary>
    /// Gets the invalid CPF value.
    /// </summary>
    public string Cpf { get; }

    /// <summary>
    /// Gets the specific reason why the CPF is invalid.
    /// </summary>
    public string? Reason { get; }
}
