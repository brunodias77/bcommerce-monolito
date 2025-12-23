namespace Bcommerce.BuildingBlocks.Security.Authentication;

/// <summary>
/// Contrato para serviços de hash de senhas.
/// </summary>
/// <remarks>
/// Define métodos para criar e verificar hashes seguros.
/// - Abstrai o algoritmo de hash subjacente
/// - Garante consistência na segurança de credenciais
/// 
/// Exemplo de uso:
/// <code>
/// // Injeção de dependência
/// public class MyService(IPasswordHasher hasher) { ... }
/// </code>
/// </remarks>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}
