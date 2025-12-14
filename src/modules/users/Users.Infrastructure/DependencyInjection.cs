using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Core.Repositories;
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
        // DbContext
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsHistoryTable("__EFMigrationsHistory", "users")
            ));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();
        services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();

        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
