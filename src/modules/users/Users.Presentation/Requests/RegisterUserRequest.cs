using System.ComponentModel.DataAnnotations;

namespace Users.Presentation.Requests;

/// <summary>
/// Request para registro de novo usuário.
/// </summary>
public record RegisterUserRequest(
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    string Email,

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    string Password,

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    string ConfirmPassword,

    [MaxLength(100)]
    string? FirstName,

    [MaxLength(100)]
    string? LastName
);
