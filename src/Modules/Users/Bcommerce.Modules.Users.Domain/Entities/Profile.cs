using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Users.Domain.ValueObjects;

namespace Bcommerce.Modules.Users.Domain.Entities;

public class Profile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    
    // Dados Pessoais
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Gender { get; private set; }
    public Cpf? Cpf { get; private set; }

    // Preferências
    public string PreferredLanguage { get; private set; } = "pt-BR";
    public string PreferredCurrency { get; private set; } = "BRL";
    public bool NewsletterSubscribed { get; private set; }

    // Termos
    public DateTime? AcceptedTermsAt { get; private set; }
    public DateTime? AcceptedPrivacyAt { get; private set; }
    
    // Controle de Concorrência
    public int Version { get; private set; }

    // Construtor vazio para EF Core
    protected Profile() { }

    public Profile(
        Guid userId, 
        string firstName, 
        string lastName, 
        string displayName, 
        Cpf? cpf = null,
        DateOnly? birthDate = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        Cpf = cpf;
        BirthDate = birthDate;
        Version = 1;
    }

    public void UpdatePersonalData(string firstName, string lastName, string displayName, DateOnly? birthDate, string? gender)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        BirthDate = birthDate;
        Gender = gender;
        // Disparar evento se necessário
    }

    public void SetAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }

    public void SetCpf(Cpf cpf)
    {
        Cpf = cpf;
    }

    public void AcceptTerms()
    {
        AcceptedTermsAt = DateTime.UtcNow;
        AcceptedPrivacyAt = DateTime.UtcNow;
    }
}
