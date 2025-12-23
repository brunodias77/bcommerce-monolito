using System.Security.Cryptography;

namespace Bcommerce.BuildingBlocks.Security.Authentication;

/// <summary>
/// Implementação padrão de hash de senhas.
/// </summary>
/// <remarks>
/// Utiliza PBKDF2 com SHA256 para segurança.
/// - Gera salt aleatório para cada hash
/// - Verifica senha contra hash armazenado (tempo constante)
/// 
/// Exemplo de uso:
/// <code>
/// var hash = _passwordHasher.Hash("senha123");
/// </code>
/// </remarks>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private const char Delimiter = ';';

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string hashedPassword)
    {
        var elements = hashedPassword.Split(Delimiter);
        var salt = Convert.FromBase64String(elements[0]);
        var hash = Convert.FromBase64String(elements[1]);

        var hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, hashInput);
    }
}
