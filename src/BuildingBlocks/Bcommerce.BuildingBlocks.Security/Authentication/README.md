# Security Authentication

Este diretório contém os componentes responsáveis pela autenticação, geração de tokens e segurança de credenciais (senhas).

## `IPasswordHasher.cs`
**Responsabilidade:** Contrato para o serviço de hash e verificação de senhas.
**Por que existe:** Para abstrair o algoritmo de criptografia utilizado, permitindo trocas futuras (ex: de PBKDF2 para Argon2) sem impactar o código de negócio.
**Em que situação usar:** Injetado em Application Services que lidam com cadastro ou login de usuários.
**O que pode dar errado:** Se a implementação concreta não for registrada no DI, o serviço falhará no startup.
**Exemplo real de uso:**
```csharp
public class RegisterUserHandler(IPasswordHasher hasher) {
    var hash = hasher.Hash(command.Password);
}
```

---

## `PasswordHasher.cs`
**Responsabilidade:** Implementação concreta de hash de senhas utilizando o algoritmo padronizado PBKDF2 com SHA256.
**Por que existe:** Armazenar senhas em texto plano é uma falha grave de segurança. Esta classe garante que apenas o hash + salt sejam persistidos.
**Em que situação usar:** Configurado no DI como a implementação de `IPasswordHasher`.
**O que pode dar errado:**
- **Alteração de parâmetros:** Mudar o número de iterações ou tamanho do salt quebrará a verificação de todas as senhas antigas.
- **Performance:** Um número de iterações excessivamente alto pode causar DoS (Denial of Service) por consumo de CPU no login.
**Exemplo real de uso:**
```csharp
bool isValid = _passwordHasher.Verify("senha123", "saltBase64;hashBase64");
```

---

## `ITokenGenerator.cs`
**Responsabilidade:** Contrato para geração de tokens de acesso (Access Tokens).
**Por que existe:** Para desacoplar a lógica de negócio (Login) da tecnologia de token (JWT, PASETO, Reference Tokens).
**Em que situação usar:** No momento que as credenciais do usuário são validadas com sucesso.
**O que pode dar errado:** N/A (Interface).
**Exemplo real de uso:**
```csharp
var token = _tokenGenerator.GenerateToken(user.Id, user.Name, ...);
```

---

## `JwtTokenGenerator.cs`
**Responsabilidade:** Gerar tokens no padrão JWT (JSON Web Token) assinados simetricamente (HMAC-SHA256).
**Por que existe:** É o padrão de mercado para autenticação stateless em APIs REST. Transmite claims do usuário de forma segura.
**Em que situação usar:** Quando a aplicação precisar emitir um Bearer Token para o client.
**O que pode dar errado:**
- **Segurança:** Nunca colocar dados sensíveis (senhas, cartões) nas claims, pois o JWT é apenas codificado em Base64, não criptografado (qualquer um pode ler o payload).
- **Chave Fraca:** Se a `Secret` no `JwtSettings` for curta, ela pode ser descoberta via força bruta, permitindo falsificação de tokens.
**Exemplo real de uso:**
```csharp
// Retorna: "eyJhbGciOiJIUzI1NiIsInR5..."
string jwt = _jwtGenerator.GenerateToken(uid, "Bruno", "Dias", "email@teste.com", perms, roles);
```

---

## `JwtSettings.cs`
**Responsabilidade:** Representar fortemente tipada as configurações de JWT do `appsettings.json`.
**Por que existe:** Para evitar "magic strings" e buscar configurações de forma segura e validada.
**Em que situação usar:** Configurado via IOptions pattern no startup.
**O que pode dar errado:** Se o `ExpiryMinutes` for muito longo, um token roubado poderá ser usado por muito tempo. Se for muito curto, prejudica a UX (logouts constantes).
**Exemplo real de uso:**
```json
"JwtSettings": {
  "Secret": "SuperSeguraChaveSecreta123!",
  "ExpiryMinutes": 60
}
```

---

## `RefreshTokenService.cs`
**Responsabilidade:** Gerar tokens de atualização (Refresh Tokens) opacos e criptograficamente seguros.
**Por que existe:** O JWT tem vida curta. O Refresh Token permite obter um novo JWT sem reenviar a senha. Ele deve ser uma string aleatória imprevisível, não um JWT.
**Em que situação usar:** Junto com a emissão do Access Token (JWT).
**O que pode dar errado:** Usar a classe `Random` do .NET gera números previsíveis. Esta classe usa `RandomNumberGenerator` (CSPRNG) para garantir entropia real.
**Exemplo real de uso:**
```csharp
var refreshToken = _refreshTokenService.GenerateRefreshToken();
// Salvar no banco associado ao usuário
```
