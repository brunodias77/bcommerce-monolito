using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Repositories;
using Bcommerce.Modules.Users.Domain.Services;
using Bcommerce.Modules.Users.Infrastructure.Identity;
using Bcommerce.Modules.Users.Infrastructure.Persistence;
using Bcommerce.Modules.Users.Infrastructure.Persistence.Repositories;
using Bcommerce.Modules.Users.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Users.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

        // Identity Configuration
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();
        
        services.ConfigureIdentity(); // Extension method in IdentityConfiguration

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>(); // Missing repo impl?

        // Identity wrappers and Services
        services.AddScoped<IUserDomainService, IdentityService>();
        services.AddScoped<EmailService>();
        services.AddScoped<SmsService>();

        return services;
    }
}
