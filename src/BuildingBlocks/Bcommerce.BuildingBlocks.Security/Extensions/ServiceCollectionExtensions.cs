using System.Text;
using Bcommerce.BuildingBlocks.Security.Authentication;
using Bcommerce.BuildingBlocks.Security.Authorization.Handlers;
using Bcommerce.BuildingBlocks.Security.Authorization.Requirements;
using Bcommerce.BuildingBlocks.Security.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bcommerce.BuildingBlocks.Security.Extensions;

/// <summary>
/// Extensões para configuração de segurança no DI.
/// </summary>
/// <remarks>
/// Registra serviços de autenticação e autorização.
/// - Configura JWT Bearer Authentication
/// - Registra policies e handlers de autorização
/// - Configura serviços utilitários (PasswordHasher, TokenGenerator)
/// 
/// Exemplo de uso:
/// <code>
/// services.AddSecurityServices(configuration);
/// </code>
/// </remarks>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));

        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });
            
        services.AddAuthorization(options =>
        {
             // Add global policies here if needed
        });

        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IAuthorizationHandler, ModuleAccessHandler>();

        return services;
    }
}
