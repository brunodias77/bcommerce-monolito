using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.ValueObjects;

public class Cpf : ValueObject
{
    public string Value { get; }

    private Cpf(string value)
    {
        Value = value;
    }

    public static Cpf Create(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ArgumentException("CPF cannot be empty.", nameof(cpf));
        }

        // Remove caracteres não numéricos
        var digitsOnly = new string(cpf.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length != 11)
        {
            throw new ArgumentException("CPF must have 11 digits.", nameof(cpf));
        }

        if (IsAllDigitsEqual(digitsOnly) || !IsValidChecksum(digitsOnly))
        {
            throw new ArgumentException("Invalid CPF.", nameof(cpf));
        }

        // Armazenar formatado ou limpo? Schema tem Check constraint para formatado: ^\d{3}\.\d{3}\.\d{3}-\d{2}$
        // Vamos formatar para garantir consistência com o banco
        var formatted = Convert.ToUInt64(digitsOnly).ToString(@"000\.000\.000\-00");

        return new Cpf(formatted);
    }
    
    public string GetSanitized() => new(Value.Where(char.IsDigit).ToArray());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    private static bool IsAllDigitsEqual(string cpf) => cpf.Distinct().Count() == 1;

    private static bool IsValidChecksum(string cpf)
    {
        int[] firstMultiplier = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] secondMultiplier = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var tempCpf = cpf[..9];
        var sum = 0;

        for (var i = 0; i < 9; i++)
        {
            sum += (tempCpf[i] - '0') * firstMultiplier[i];
        }

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        tempCpf += firstDigit;
        sum = 0;

        for (var i = 0; i < 10; i++)
        {
            sum += (tempCpf[i] - '0') * secondMultiplier[i];
        }

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return cpf.EndsWith($"{firstDigit}{secondDigit}");
    }
}
