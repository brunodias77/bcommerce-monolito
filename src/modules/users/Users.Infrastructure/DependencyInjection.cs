using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Core.Entities;
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
        // ===============================================================
        // DbContext
        // ===============================================================
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsHistoryTable("__EFMigrationsHistory", "users")
            ));

        // ===============================================================
        // ASP.NET Identity
        // ===============================================================
        // Data Protection é necessário para os TokenProviders
        services.AddDataProtection();

        services.AddIdentityCore<User>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

        // ===============================================================
        // Repositories
        // ===============================================================
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();
        services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();

        // ===============================================================
        // Services
        // ===============================================================
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
