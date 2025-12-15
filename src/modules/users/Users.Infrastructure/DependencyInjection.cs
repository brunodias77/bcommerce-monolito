using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Users.Core.Entities;
using Users.Core.Repositories;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Infrastructure.Messaging.Integration;
using BuildingBlocks.Domain.Events;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Services;

namespace Users.Infrastructure;

/// <summary>
/// Extensão para registrar os serviços do módulo Users.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ===============================================================
        // DATABASE CONTEXT (PostgreSQL)
        // ===============================================================
        // Configura o EF Core para usar Npgsql.
        // NOTA: É crucial definir a tabela de histórico de migrações com um nome específico ("__EFMigrationsHistory")
        // e, PRINCIPALMENTE, no schema correto ("users"). Isso permite que cada módulo gerencie
        // suas próprias migrações de forma isolada sem conflitos na mesma tabela global.
        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsHistoryTable("__EFMigrationsHistory", "users")
            );

            // Registro de Interceptors
            // Eles interceptam o SaveChanges para aplicar lógicas transversais automaticamente.
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OptimisticConcurrencyInterceptor>(),
                sp.GetRequiredKeyedService<PublishDomainEventsInterceptor>("users")
            );
        });

        // ===============================================================
        // ASP.NET CORE IDENTITY
        // ===============================================================
        // Configuração do sistema de identidade (Users, Roles, Claims, Logins).
        
        // Data Protection: Necessário para criptografar tokens (Reset Password, Email Confirmation).
        // Sem isso, os tokens gerados podem ser inválidos em ambientes distribuídos.
        services.AddDataProtection();

        services.AddIdentityCore<User>(options =>
            {
                // POLÍTICA DE SENHAS
                // Define a complexidade exigida para novas senhas.
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;           // Ex: 123
                options.Password.RequireLowercase = false;      // Ex: abc (opcional)
                options.Password.RequireUppercase = true;       // Ex: ABC
                options.Password.RequireNonAlphanumeric = false; // Caracteres especiais (!@#)

                // LOCKOUT (Bloqueio de conta)
                // Protege contra ataques de força bruta.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // OUTRAS CONFIGURAÇÕES
                options.User.RequireUniqueEmail = true; // Email deve ser único no sistema
            })
            .AddEntityFrameworkStores<UsersDbContext>() // Armazena dados no nosso DbContext customizado
            .AddDefaultTokenProviders();                // Provedores de token para Email/SMS/Totp

        // ===============================================================
        // REPOSITÓRIOS (Data Access)
        // ===============================================================
        // Registrados como Scoped para acompanhar o tempo de vida da requisição HTTP/Transação.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();
        services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();

        // ===============================================================
        // MESSAGING (Event Bus)
        // ===============================================================
        // Registra o OutboxEventBus como Keyed Service para o módulo Users.
        // Isso permite injetar IEventBus especificamente para este módulo usando [FromKeyedServices("users")]
        services.AddKeyedScoped<IEventBus, OutboxEventBus>("users", (sp, key) =>
        {
            var dbContext = sp.GetRequiredService<UsersDbContext>();
            var logger = sp.GetService<ILogger<OutboxEventBus>>(); // Opcional
            return new OutboxEventBus(dbContext, "users", logger);
        });

        // ===============================================================
        // DOMAIN SERVICES & INFRASTRUCTURE SERVICES
        // ===============================================================
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
