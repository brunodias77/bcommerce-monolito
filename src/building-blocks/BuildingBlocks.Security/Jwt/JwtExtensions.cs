using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Security.Jwt;

/// <summary>
/// Extensões para configurar autenticação JWT
/// </summary>
public static class JwtExtensions
{
    /// <summary>
    /// Adiciona autenticação JWT usando Bearer token
    /// Configura validação de tokens de acordo com as opções fornecidas
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Carrega configurações de JWT
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        // Valida configurações obrigatórias
        ValidarConfiguracoes(jwtOptions);

        // Registra as opções para injeção de dependência
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        // Configura autenticação
        services.AddAuthentication(options =>
        {
            // Define JWT Bearer como esquema padrão
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Parâmetros de validação do token
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Valida o emissor (issuer)
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidIssuer = jwtOptions.Issuer,

                // Valida a audiência (audience)
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidAudience = jwtOptions.Audience,

                // Valida o tempo de vida do token
                ValidateLifetime = jwtOptions.ValidateLifetime,

                // Valida a chave de assinatura
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                // Tolerância de tempo (clock skew)
                ClockSkew = TimeSpan.FromMinutes(jwtOptions.ClockSkewMinutes),

                // Define qual claim usar para o Name (padrão seria ClaimTypes.Name)
                NameClaimType = ClaimTypes.Name,

                // Define qual claim usar para o Role
                RoleClaimType = ClaimTypes.Role
            };

            // Salva o token no AuthenticationProperties
            options.SaveToken = jwtOptions.SaveToken;

            // Inclui detalhes de erro (apenas em desenvolvimento)
            options.IncludeErrorDetails = jwtOptions.IncludeErrorDetails;

            // Eventos do JWT Bearer
            options.Events = new JwtBearerEvents
            {
                // Chamado quando a autenticação falha
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        // Token expirado
                        context.Response.Headers.Append("Token-Expirado", "true");
                    }

                    return Task.CompletedTask;
                },

                // Chamado quando o token é validado com sucesso
                OnTokenValidated = context =>
                {
                    // Aqui você pode adicionar lógica adicional após validação
                    // Por exemplo: verificar se o usuário ainda está ativo no banco

                    return Task.CompletedTask;
                },

                // Chamado quando um desafio de autenticação é retornado
                OnChallenge = context =>
                {
                    // Customiza a resposta de erro 401
                    return Task.CompletedTask;
                },

                // Chamado quando recebe uma mensagem
                OnMessageReceived = context =>
                {
                    // Permite receber token de query string (útil para SignalR)
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Gera um JWT token para um usuário
    /// Usado após login bem-sucedido
    /// </summary>
    /// <param name="userId">ID do usuário (Guid de users.asp_net_users)</param>
    /// <param name="email">Email do usuário</param>
    /// <param name="userName">Nome de usuário</param>
    /// <param name="roles">Papéis do usuário (de users.asp_net_user_roles)</param>
    /// <param name="additionalClaims">Claims adicionais (opcional)</param>
    /// <param name="jwtOptions">Opções de JWT</param>
    /// <returns>Token JWT assinado</returns>
    public static string GerarToken(
        Guid userId,
        string email,
        string userName,
        IEnumerable<string> roles,
        IDictionary<string, string>? additionalClaims,
        JwtOptions jwtOptions)
    {
        // Claims padrão
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Issued At
        };

        // Adiciona roles como claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Adiciona claims adicionais
        if (additionalClaims != null)
        {
            claims.AddRange(additionalClaims.Select(kvp => new Claim(kvp.Key, kvp.Value)));
        }

        // Chave de assinatura
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Cria o token
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationInMinutes),
            signingCredentials: credentials
        );

        // Serializa o token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Valida e extrai claims de um token JWT
    /// </summary>
    /// <param name="token">Token JWT</param>
    /// <param name="jwtOptions">Opções de JWT</param>
    /// <returns>ClaimsPrincipal com os claims do token</returns>
    public static ClaimsPrincipal? ValidarToken(string token, JwtOptions jwtOptions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ClockSkew = TimeSpan.FromMinutes(jwtOptions.ClockSkewMinutes)
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gera um refresh token seguro (aleatório)
    /// Deve ser armazenado em users.sessions com hash
    /// </summary>
    public static string GerarRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Valida configurações de JWT
    /// Lança exceção se configurações obrigatórias estiverem faltando
    /// </summary>
    private static void ValidarConfiguracoes(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Secret))
        {
            throw new InvalidOperationException(
                "A chave secreta do JWT (Jwt:Secret) não está configurada");
        }

        if (options.Secret.Length < 32)
        {
            throw new InvalidOperationException(
                "A chave secreta do JWT deve ter pelo menos 32 caracteres");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException(
                "O emissor do JWT (Jwt:Issuer) não está configurado");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException(
                "A audiência do JWT (Jwt:Audience) não está configurada");
        }

        if (options.ExpirationInMinutes <= 0)
        {
            throw new InvalidOperationException(
                "O tempo de expiração do JWT deve ser maior que zero");
        }
    }
}
